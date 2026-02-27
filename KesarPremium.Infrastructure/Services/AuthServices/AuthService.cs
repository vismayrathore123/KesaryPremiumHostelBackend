using KesarPremium.Core.DTOs.Request;
using KesarPremium.Core.DTOs.Response;
using KesarPremium.Core.Entities;
using KesarPremium.Core.Interfaces.IRepositories;
using KesarPremium.Core.Interfaces.IServices;
using KesarPremium.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Infrastructure.Services.AuthServices
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRefreshTokenRepository _rtRepo;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _config;

        public AuthService(IUserRepository userRepo, IRefreshTokenRepository rtRepo,
            ITokenService tokenService, IConfiguration config)
        {
            _userRepo = userRepo;
            _rtRepo = rtRepo;
            _tokenService = tokenService;
            _config = config;
        }

        public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest req)
        {
            if (await _userRepo.EmailExistsAsync(req.Email))
                return ApiResponse<AuthResponse>.Fail("Email is already registered.");

            var user = new User
            {
                FullName = req.FullName,
                Email = req.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                PhoneNumber = req.PhoneNumber,
                RoleId = 2
            };

            await _userRepo.AddAsync(user);

            // Reload with Role for token generation
            var saved = await _userRepo.GetByEmailAsync(req.Email);
            return await BuildAuthResponseAsync(saved!);
        }

        public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest req)
        {
            var user = await _userRepo.GetByEmailAsync(req.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
                return ApiResponse<AuthResponse>.Fail("Invalid email or password.");

            if (!user.IsActive)
                return ApiResponse<AuthResponse>.Fail("Your account has been deactivated. Please contact admin.");

            return await BuildAuthResponseAsync(user);
        }

        public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(string refreshToken)
        {
            var token = await _rtRepo.GetByTokenAsync(refreshToken);
            if (token == null || token.IsRevoked || token.ExpiresAt < DateTime.UtcNow)
                return ApiResponse<AuthResponse>.Fail("Invalid or expired refresh token.");

            await _rtRepo.RevokeAsync(refreshToken);
            return await BuildAuthResponseAsync(token.User);
        }

        public async Task<ApiResponse<bool>> LogoutAsync(string refreshToken)
        {
            await _rtRepo.RevokeAsync(refreshToken);
            return ApiResponse<bool>.Ok(true, "Logged out successfully.");
        }

        private async Task<ApiResponse<AuthResponse>> BuildAuthResponseAsync(User user)
        {
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var expiryDays = double.Parse(_config["JwtSettings:RefreshTokenExpiryDays"]!);

            await _rtRepo.AddAsync(new RefreshToken
            {
                UserId = user.UserId,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(expiryDays)
            });

            return ApiResponse<AuthResponse>.Ok(new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(double.Parse(_config["JwtSettings:AccessTokenExpiryMinutes"]!)),
                User = new UserDto
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    RoleName = user.Role?.RoleName ?? "User",
                    ProfilePicture = user.ProfilePicture
                }
            }, "Authentication successful.");
        }
    }

}

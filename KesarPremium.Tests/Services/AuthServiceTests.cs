using KesarPremium.Core.DTOs.Request;
using KesarPremium.Core.Entities;
using KesarPremium.Core.Interfaces;
using KesarPremium.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace KesarPremium.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IRefreshTokenRepository> _rtRepoMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _rtRepoMock = new Mock<IRefreshTokenRepository>();
        _tokenServiceMock = new Mock<ITokenService>();
        _configMock = new Mock<IConfiguration>();

        // Setup config defaults
        _configMock.Setup(c => c["JwtSettings:AccessTokenExpiryMinutes"]).Returns("15");
        _configMock.Setup(c => c["JwtSettings:RefreshTokenExpiryDays"]).Returns("7");

        _authService = new AuthService(
            _userRepoMock.Object,
            _rtRepoMock.Object,
            _tokenServiceMock.Object,
            _configMock.Object
        );
    }

    // ── REGISTER ──────────────────────────────────────
    [Fact]
    public async Task Register_WithNewEmail_ShouldReturnSuccess()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FullName = "Test User",
            Email = "test@example.com",
            Password = "Test@123",
            PhoneNumber = "9876543210"
        };

        _userRepoMock.Setup(r => r.EmailExistsAsync(request.Email)).ReturnsAsync(false);
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);
        _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync(new User
        {
            UserId = 1,
            FullName = request.FullName,
            Email = request.Email,
            RoleId = 2,
            Role = new Role { RoleId = 2, RoleName = "User" }
        });

        _tokenServiceMock.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("mock_access_token");
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("mock_refresh_token");
        _rtRepoMock.Setup(r => r.AddAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("mock_access_token", result.Data.AccessToken);
        Assert.Equal(request.FullName, result.Data.User.FullName);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ShouldReturnFailure()
    {
        // Arrange
        var request = new RegisterRequest { Email = "existing@example.com", Password = "Test@123", FullName = "Test" };
        _userRepoMock.Setup(r => r.EmailExistsAsync(request.Email)).ReturnsAsync(true);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("already registered", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ── LOGIN ─────────────────────────────────────────
    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var password = "Test@123";
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        var request = new LoginRequest { Email = "user@example.com", Password = password };

        _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync(new User
        {
            UserId = 1,
            Email = request.Email,
            PasswordHash = hash,
            IsActive = true,
            Role = new Role { RoleId = 2, RoleName = "User" }
        });

        _tokenServiceMock.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("access_token");
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("refresh_token");
        _rtRepoMock.Setup(r => r.AddAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("access_token", result.Data!.AccessToken);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturnFailure()
    {
        // Arrange
        var request = new LoginRequest { Email = "user@example.com", Password = "WrongPassword" };
        _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync(new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword"),
            IsActive = true
        });

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid", result.Message);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ShouldReturnFailure()
    {
        // Arrange
        var request = new LoginRequest { Email = "notexist@example.com", Password = "Test@123" };
        _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync((User?)null);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.False(result.Success);
    }

    [Fact]
    public async Task Login_WithDeactivatedAccount_ShouldReturnFailure()
    {
        // Arrange
        var request = new LoginRequest { Email = "inactive@example.com", Password = "Test@123" };
        _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync(new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
            IsActive = false
        });

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("deactivated", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ── LOGOUT ────────────────────────────────────────
    [Fact]
    public async Task Logout_WithValidToken_ShouldReturnSuccess()
    {
        // Arrange
        _rtRepoMock.Setup(r => r.RevokeAsync("valid_refresh_token")).Returns(Task.CompletedTask);

        // Act
        var result = await _authService.LogoutAsync("valid_refresh_token");

        // Assert
        Assert.True(result.Success);
        _rtRepoMock.Verify(r => r.RevokeAsync("valid_refresh_token"), Times.Once);
    }

    // ── REFRESH TOKEN ─────────────────────────────────
    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange
        var user = new User
        {
            UserId = 1,
            Email = "user@example.com",
            Role = new Role { RoleName = "User" }
        };

        _rtRepoMock.Setup(r => r.GetByTokenAsync("valid_token")).ReturnsAsync(new RefreshToken
        {
            Token = "valid_token",
            IsRevoked = false,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            User = user
        });

        _rtRepoMock.Setup(r => r.RevokeAsync("valid_token")).Returns(Task.CompletedTask);
        _rtRepoMock.Setup(r => r.AddAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);
        _tokenServiceMock.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("new_access_token");
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("new_refresh_token");

        // Act
        var result = await _authService.RefreshTokenAsync("valid_token");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("new_access_token", result.Data!.AccessToken);
    }

    [Fact]
    public async Task RefreshToken_WithExpiredToken_ShouldReturnFailure()
    {
        // Arrange
        _rtRepoMock.Setup(r => r.GetByTokenAsync("expired_token")).ReturnsAsync(new RefreshToken
        {
            Token = "expired_token",
            IsRevoked = false,
            ExpiresAt = DateTime.UtcNow.AddDays(-1)  // expired yesterday
        });

        // Act
        var result = await _authService.RefreshTokenAsync("expired_token");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("expired", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RefreshToken_WithRevokedToken_ShouldReturnFailure()
    {
        // Arrange
        _rtRepoMock.Setup(r => r.GetByTokenAsync("revoked_token")).ReturnsAsync(new RefreshToken
        {
            Token = "revoked_token",
            IsRevoked = true,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });

        // Act
        var result = await _authService.RefreshTokenAsync("revoked_token");

        // Assert
        Assert.False(result.Success);
    }
}

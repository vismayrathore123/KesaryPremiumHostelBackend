using KesarPremium.Core.Entities;
using KesarPremium.Core.Interfaces.IRepositories;
using KesarPremium.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _db;
        public RefreshTokenRepository(ApplicationDbContext db) => _db = db;

        public async Task<RefreshToken?> GetByTokenAsync(string token)
            => await _db.RefreshTokens.Include(t => t.User).ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(t => t.Token == token);

        public async Task AddAsync(RefreshToken token) { _db.RefreshTokens.Add(token); await _db.SaveChangesAsync(); }

        public async Task RevokeAsync(string token)
        {
            var t = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);
            if (t != null) { t.IsRevoked = true; await _db.SaveChangesAsync(); }
        }

        public async Task RevokeAllUserTokensAsync(int userId)
        {
            var tokens = await _db.RefreshTokens.Where(t => t.UserId == userId && !t.IsRevoked).ToListAsync();
            tokens.ForEach(t => t.IsRevoked = true);
            await _db.SaveChangesAsync();
        }
    }

}

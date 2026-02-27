using KesarPremium.Core.Entities;
using KesarPremium.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext db) : base(db) { }

        public async Task<User?> GetByEmailAsync(string email)
            => await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == email);

        public async Task<bool> EmailExistsAsync(string email)
            => await _db.Users.AnyAsync(u => u.Email == email);
    }
}

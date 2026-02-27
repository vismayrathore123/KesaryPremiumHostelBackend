using KesarPremium.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KesarPremium.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _db;
        public Repository(ApplicationDbContext db) => _db = db;

        public async Task<T?> GetByIdAsync(int id) => await _db.Set<T>().FindAsync(id);
        public async Task<IEnumerable<T>> GetAllAsync() => await _db.Set<T>().ToListAsync();
        public async Task<T> AddAsync(T entity) { _db.Set<T>().Add(entity); await _db.SaveChangesAsync(); return entity; }
        public async Task UpdateAsync(T entity) { _db.Set<T>().Update(entity); await _db.SaveChangesAsync(); }
        public async Task DeleteAsync(T entity) { _db.Set<T>().Remove(entity); await _db.SaveChangesAsync(); }
        public async Task<bool> ExistsAsync(int id) => await _db.Set<T>().FindAsync(id) != null;
    }

}

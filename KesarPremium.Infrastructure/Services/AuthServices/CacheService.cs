using KesarPremium.Core.Interfaces.IServices;
using Microsoft.EntityFrameworkCore.Storage;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KesarPremium.Infrastructure.Services.AuthServices
{//REdis
    public class CacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public CacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _db = redis.GetDatabase();
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var val = await _db.StringGetAsync(key);
            return val.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(val!);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var json = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(key, json, expiry ?? TimeSpan.FromMinutes(10));
        }

        public async Task RemoveAsync(string key) => await _db.KeyDeleteAsync(key);

        public async Task RemoveByPatternAsync(string pattern)
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: pattern + "*").ToArray();
            if (keys.Length > 0) await _db.KeyDeleteAsync(keys);
        }
    }

}

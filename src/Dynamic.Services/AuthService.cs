using Dynamic.DAL.Entities.Authorization;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace Dynamic.Services
{
    public class AuthService : IAuthService
    {
        private readonly IMemoryCache _memoryCache;

        public AuthService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        public bool IsAuthorized(string tableName, string roleName, string httpMethod)
        {
            if (_memoryCache.TryGetValue<Permission>($"{tableName}_{roleName}_{httpMethod}", out var permission))
            {
                return permission.Enabled;
            }

            return false;
        }
    }
}
using System.Collections.Concurrent;

namespace GateHub.repository
{
    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly ConcurrentDictionary<string, DateTime> _blacklist = new();

        public Task<bool> IsTokenBlacklisted(string token)
        {
            if (_blacklist.TryGetValue(token, out var expiry))
            {
                return expiry >= DateTime.UtcNow
                    ? Task.FromResult(true)
                    : Task.FromResult(!_blacklist.TryRemove(token, out _));
            }
            return Task.FromResult(false);
        }

        public Task BlacklistToken(string token, DateTime expiry)
        {
            _blacklist.TryAdd(token, expiry);
            return Task.CompletedTask;
        }
    }
}

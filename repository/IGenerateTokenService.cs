using GateHub.Models;

namespace GateHub.repository
{
    public interface IGenerateTokenService
    {
        public Task<string> GenerateJwtTokenAsync(AppUser user);
    }
}

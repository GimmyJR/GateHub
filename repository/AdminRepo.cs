using GateHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Unicode;

namespace GateHub.repository
{
    public class AdminRepo : IAdminRepo
    {
        private readonly UserManager<AppUser> userManager;
        private readonly IConfiguration configuration;

        public AdminRepo(UserManager<AppUser> userManager,IConfiguration configuration)
        {
            this.userManager = userManager;
            this.configuration = configuration;
        }

        public async Task<string> GenerateJwtTokenAsync(AppUser user)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,user.Name),
                new Claim(ClaimTypes.DateOfBirth,user.BirthDate.ToString()),
                new Claim(ClaimTypes.MobilePhone,user.PhoneNumber),
                new Claim(ClaimTypes.SerialNumber,user.NatId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var userRoles = await userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: configuration["JWT:VallidIssuer"],
                audience: configuration["JWT:VallidAudience"],
                expires:DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(key,SecurityAlgorithms.HmacSha256)
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
    public interface IAdminRepo
    {
        Task<string> GenerateJwtTokenAsync(AppUser user);
    }
}

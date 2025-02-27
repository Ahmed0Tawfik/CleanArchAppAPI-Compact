using CleanArch.Application.Models.Auth;
using System.Security.Claims;

namespace CleanArch.Application.Interfaces.Authentication
{
    public interface ITokenService
    {
        Task<TokenResponse> GenerateTokenAsync(string userId, string userName, IEnumerable<string> roles = null, IEnumerable<Claim> additionalClaims = null);
        Task<ClaimsPrincipal> GetPrincipalFromTokenAsync(string token);
        Task<string> GenerateRefreshTokenAsync();
        Task<bool> ValidateRefreshTokenAsync(string refreshToken);
    }
}

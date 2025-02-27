
using CleanArch.Application.Auth;
using CleanArch.Application.Models.Auth;

namespace CleanArch.Application.Interfaces.Authentication
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(UserLoginCommand request);
        Task<AuthResponse> RegisterAsync(UserRegisterCommand request);
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task<bool> ValidateTokenAsync(string token);
        Task<bool> RevokeRefreshTokenAsync(string refreshToken);
    }
}

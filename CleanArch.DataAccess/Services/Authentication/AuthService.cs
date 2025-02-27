
using CleanArch.Application.Auth;
using CleanArch.Application.Interfaces.Authentication;
using CleanArch.Application.Models.Auth;
using CleanArch.Domain.Exceptions;
using CleanArch.Infrastructure.Context;
using CleanArch.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace CleanArch.Infrastructure.Services.Authentication
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly ApplicationDbContext _dbContext;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _dbContext = dbContext;
        }

        public async Task<AuthResponse> LoginAsync(UserLoginCommand request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new AuthenticationFailureException("Invalid credentials");
            }

            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!signInResult.Succeeded)
            {
                throw new AuthenticationFailureException("Invalid credentials");
            }

            return await GenerateAuthenticationResultForUserAsync(user);
        }

        public async Task<AuthResponse> RegisterAsync(UserRegisterCommand request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new AuthenticationFailureException("Account Already Exists");
            }

            var existingUserName = await _userManager.FindByNameAsync(request.UserName);
            if (existingUserName != null)
            {
                throw new AuthenticationFailureException("Account Already Exists");
            }

            var newUser = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.UserName
            };

            var createdUser = await _userManager.CreateAsync(newUser, request.Password);
            if (!createdUser.Succeeded)
            {
                throw new AuthenticationFailureException("Account Creation Error");
            }

            //await _userManager.AddToRoleAsync(newUser, "User");

            return await GenerateAuthenticationResultForUserAsync(newUser);
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var validatedToken = await _tokenService.GetPrincipalFromTokenAsync(request.RefreshToken);
            if (validatedToken == null)
            {
               throw new InvalidTokenException("Invalid token");
            }

            var expiryDateUnix = long.Parse(validatedToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
            var expiryDateTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(expiryDateUnix);

            if (expiryDateTimeUtc > DateTime.UtcNow)
            {
                throw new InvalidTokenException("Hasnt Expired Yet !");
            }

            var jti = validatedToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
            var storedRefreshToken = await _dbContext.RefreshTokens.SingleOrDefaultAsync(x => x.Token == request.RefreshToken);

            if (storedRefreshToken == null)
            {
                throw new InvalidTokenException("Refresh Token Does not Exist !");

            }

            if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
            {
                throw new InvalidTokenException("Refresh Token Has Expired");

            }

            if (storedRefreshToken.Invalidated)
            {
                throw new InvalidTokenException("This refresh token has been invalidated");

               
            }

            if (storedRefreshToken.Used)
            {
                throw new InvalidTokenException("This refresh token has been Used !");

            }

            if (storedRefreshToken.JwtId != jti)
            {
                throw new InvalidTokenException("This refresh token is Broken !");

            }

            storedRefreshToken.Used = true;
            _dbContext.RefreshTokens.Update(storedRefreshToken);
            await _dbContext.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(storedRefreshToken.UserId);
            return await GenerateAuthenticationResultForUserAsync(user);
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            var principal = await _tokenService.GetPrincipalFromTokenAsync(token);
            if (principal == null)
            {
                return false;
            }

            var expiryDateUnix = long.Parse(principal.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
            var expiryDateTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(expiryDateUnix);

            return expiryDateTimeUtc > DateTime.UtcNow;
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            var storedRefreshToken = await _dbContext.RefreshTokens.SingleOrDefaultAsync(x => x.Token == refreshToken);
            if (storedRefreshToken == null)
            {
                return false;
            }

            storedRefreshToken.Invalidated = true;
            _dbContext.RefreshTokens.Update(storedRefreshToken);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        private async Task<AuthResponse> GenerateAuthenticationResultForUserAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var tokenResult = await _tokenService.GenerateTokenAsync(user.Id, user.UserName, roles);

            return new AuthResponse
            {
                Token = tokenResult.AccessToken,
                RefreshToken = tokenResult.RefreshToken,
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email
            };
        }
    }
}

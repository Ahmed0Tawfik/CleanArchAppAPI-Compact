using CleanArch.Application.APIResponse;
using CleanArch.Application.Interfaces.Authentication;
using FluentValidation;

namespace CleanArch.Application.Auth
{
    public record UserLoginCommand(string Email, string Password) : IRequest<ApiResponse<AuthResponse>>
    {
        public class Validator : AbstractValidator<UserLoginCommand>
        {
            public Validator()
            {
                RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required").WithMessage("Invalid email format.");
                RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
            }
        }

    }


    public class UserLoginHandler(IAuthService authService) : IRequestHandler<UserLoginCommand, ApiResponse<AuthResponse>>
    {
        public async Task<ApiResponse<AuthResponse>> Handle(UserLoginCommand request, CancellationToken ct)
        {
            var result  = await authService.LoginAsync(request);
            return ApiResponse<AuthResponse>.Success(result, "Logged On Successfully");
        }
    }
}

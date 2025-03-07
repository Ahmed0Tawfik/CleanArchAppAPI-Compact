using CleanArch.Application.Common.APIResponse;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Features.Auth.Models;
using CleanArch.Application.Interfaces.Authentication;
using FluentValidation;

namespace CleanArch.Application.Features.Auth.Commands
{

    public record UserRegisterCommand(string Email, string UserName, string Password, string ConfirmPassword) : IRequest<ApiResponse<AuthResponse>>
    {
        public class Validator : AbstractValidator<UserRegisterCommand>
        {
            public Validator()
            {
                RuleFor(x => x.Email)
                    .NotEmpty().WithMessage("Email is required.")
                    .EmailAddress().WithMessage("Invalid email format.");

                RuleFor(x => x.UserName)
                    .NotEmpty().WithMessage("Username is required.")
                    .MinimumLength(3).WithMessage("Username must be at least 3 characters long.");

                RuleFor(x => x.Password)
                    .NotEmpty().WithMessage("Password is required.")
                    .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");

                RuleFor(x => x.ConfirmPassword)
                    .Equal(x => x.Password).WithMessage("Passwords do not match.");
            }
        }


    }

    public class UserRegisterHandler(IAuthService authService) : IRequestHandler<UserRegisterCommand, ApiResponse<AuthResponse>>
    {
        public async Task<ApiResponse<AuthResponse>> Handle(UserRegisterCommand request, CancellationToken ct)
        {
            var result = await authService.RegisterAsync(request);

            return ApiResponse<AuthResponse>.Success(result, "Account Created Successfully");
        }
    }
}

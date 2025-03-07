using CleanArch.Domain.Interfaces;
using FluentValidation;
using CleanArch.Domain.Models;
using CleanArch.Application.Features.ProductCQ.Models;
using CleanArch.Application.Common.Interfaces;
using CleanArch.Application.Common.APIResponse;

namespace CleanArch.Application.Features.ProductCQ.Commands
{
    public record AddProductCommand(string Name, string Description, string imageUrl, decimal Price, bool IsNew) : IRequest<ApiResponse<ProductResponse>>
    {
        public class Validator : AbstractValidator<AddProductCommand>
        {
            public Validator()
            {
                RuleFor(x => x.Name).MaximumLength(100).WithMessage("Name must not exceed 100 characters.")
                    .NotEmpty().WithMessage("Name is required.");

                RuleFor(x => x.Description).MaximumLength(256).WithMessage("Description must not exceed 256 characters.")
                    .NotEmpty().WithMessage("Description is required.");

                RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than 0.");

                RuleFor(x => x.imageUrl).MaximumLength(256).WithMessage("imageUrl must not exceed 256 characters.")
                    .NotEmpty().WithMessage("ImageUrl is required.");

            }
        }

        public class AddProductHandler : IRequestHandler<AddProductCommand, ApiResponse<ProductResponse>>
        {
            private readonly IUnitOfWork _unitOfWork;
            public AddProductHandler(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork;
            }
            public async Task<ApiResponse<ProductResponse>> Handle(AddProductCommand request, CancellationToken ct)
            {
                var newproduct = new Product
                {
                    Name = request.Name,
                    Description = request.Description,
                    Price = request.Price,
                    InStock = true,
                    ImageUrl = request.imageUrl,
                    IsNew = request.IsNew
                };


                var result = await _unitOfWork.Repository<Product>().AddAsync(newproduct);

                await _unitOfWork.SaveChangesAsync();

                if (result == null)
                {
                    return ApiResponse<ProductResponse>.Error(result, "Failed to add product");
                }

                return ApiResponse<ProductResponse>.Success(new ProductResponse
                {
                    Id = result.Id,
                    Name = result.Name,
                    Description = result.Description,
                    ImageUrl = result.ImageUrl,
                    InStock = result.InStock,
                    Price = result.Price,
                    IsNew = result.IsNew
                }, "Product Added Successfully");
            }
        }
    }
}

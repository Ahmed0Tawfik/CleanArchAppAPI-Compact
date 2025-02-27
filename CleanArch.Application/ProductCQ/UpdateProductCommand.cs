using CleanArch.Application.APIResponse;
using CleanArch.Domain.Interfaces;
using CleanArch.Domain.Models;
using FluentValidation;

namespace CleanArch.Application.ProductCQ
{
    public record UpdateProductCommand(Guid Id, string Name, string Description, string ImageUrl, decimal Price) : IRequest<ApiResponse<ProductResponse>>
    {
        public class validator : AbstractValidator<UpdateProductCommand>
        {
            public validator()
            {
                RuleFor(x => x.Name).MaximumLength(100).WithMessage("Name must not exceed 100 characters.")
                    .NotEmpty().WithMessage("Name is required.");
                RuleFor(x => x.Description).MaximumLength(256).WithMessage("Description must not exceed 256 characters.")
                    .NotEmpty().WithMessage("Description is required.");
                RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than 0.");
                RuleFor(x => x.ImageUrl).MaximumLength(256).WithMessage("ImageUrl must not exceed 256 characters.")
                    .NotEmpty().WithMessage("ImageUrl is required.");    
            }
        }
    }

    public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, ApiResponse<ProductResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public UpdateProductHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ApiResponse<ProductResponse>> Handle(UpdateProductCommand request, CancellationToken ct)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(request.Id);
            if (product == null)
            {
                return ApiResponse<ProductResponse>.Error(null, "Product not found");
            }
            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.ImageUrl = request.ImageUrl;
            var result = await _unitOfWork.Repository<Product>().UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();
            if (result == null)
            {
                return ApiResponse<ProductResponse>.Error(result, "Failed to update product");
            }
            return ApiResponse<ProductResponse>.Success(new ProductResponse
            {
                Id = result.Id,
                Name = result.Name,
                Description = result.Description,
                ImageUrl = result.ImageUrl,
                Price = result.Price
            }, "Product updated successfully");
        }
    }
}

using CleanArch.Application.APIResponse;
using CleanArch.Domain.Interfaces;
using CleanArch.Domain.Models;
using FluentValidation;

namespace CleanArch.Application.ProductCQ
{
    public class ToggleInStockProductCommand : IRequest<ApiResponse<ProductResponse>>
    {
        public Guid Id { get; set; }

        public class Validator : AbstractValidator<ToggleInStockProductCommand>
        {
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
            }
        }
    }

    public class ToggleInStockProductHandler : IRequestHandler<ToggleInStockProductCommand, ApiResponse<ProductResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public ToggleInStockProductHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ApiResponse<ProductResponse>> Handle(ToggleInStockProductCommand request, CancellationToken ct)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(request.Id);
            if (product == null)
            {
                return ApiResponse<ProductResponse>.Error(null, "Product not found");
            }
            product.InStock = !product.InStock;
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
                Price = result.Price,
                InStock = result.InStock
            });
        }
    }

}

using CleanArch.Application.APIResponse;
using CleanArch.Domain.Interfaces;
using CleanArch.Domain.Models;
using FluentValidation;

namespace CleanArch.Application.ProductCQ
{
    public class DeleteProductCommnad : IRequest<ApiResponse<ProductResponse>>
    {
        public Guid Id { get; set; }
        public class Validator : AbstractValidator<DeleteProductCommnad>
        {
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty().WithMessage("Id must not be empty");
            }
        }
        public class DeleteProductHandler : IRequestHandler<DeleteProductCommnad, ApiResponse<ProductResponse>>
        {
            private readonly IUnitOfWork _unitOfWork;
            public DeleteProductHandler(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork;
            }
            public async Task<ApiResponse<ProductResponse>> Handle(DeleteProductCommnad request, CancellationToken ct)
            {
                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(request.Id);
                if (product == null)
                {
                    return ApiResponse<ProductResponse>.Error(null, "Product not found");
                }
                await _unitOfWork.Repository<Product>().DeleteAsync(product);

                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<ProductResponse>.Success(null, "Product Deleted Successfully");

            }
        }
    }
}

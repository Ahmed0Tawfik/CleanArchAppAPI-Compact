using CleanArch.Application.APIResponse;
using CleanArch.Domain.Interfaces;
using CleanArch.Domain.Models;
using FluentValidation;
using System.Linq;

namespace CleanArch.Application.ProductCQ
{
    public class GetAllProductsRequest() : IRequest<ApiResponse<PagedResponse<ProductResponse>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public bool? InStock { get; set; }
        public string? Search { get; set; }
        public bool? IsNew { get; set; }
        public class Validator : AbstractValidator<GetAllProductsRequest>
        {
            public Validator()
            {
                RuleFor(x => x.PageNumber).GreaterThan(0).WithMessage("Page number must be greater than 0.");
                RuleFor(x => x.PageSize).GreaterThan(0).WithMessage("Page size must be greater than 0.");
                RuleFor(x => x.Search).MaximumLength(50).WithMessage("Search string must be less than 50 characters.");
            }


        }
    }

    public class GetAllProductsHandler : IRequestHandler<GetAllProductsRequest, ApiResponse<PagedResponse<ProductResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetAllProductsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ApiResponse<PagedResponse<ProductResponse>>> Handle(GetAllProductsRequest request, CancellationToken ct)
        {
            int page = request.PageNumber > 0 ? request.PageNumber : 1;
            int pageSize = request.PageSize > 0 ? request.PageSize : 10;

            // Fetch paginated data
            var pagedProducts = await _unitOfWork.Repository<Product>().GetPagedAsync(request.Search, page, pageSize, request.InStock, request.IsNew);
            var Records = await _unitOfWork.Repository<Product>().GetAllAsync();

            if (pagedProducts.Data is null || !pagedProducts.Data.Any()) 
            {
                return ApiResponse<PagedResponse<ProductResponse>>.Error(null, "No products found");
            }

            // Map Product entities to ProductResponse DTOs
            var productResponses = pagedProducts.Data.Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                InStock = p.InStock,
                IsNew = p.IsNew
                // Map other properties
            }).ToList();

            // Create paged response
            var pagedResponse = new PagedResponse<ProductResponse>(productResponses, page, pageSize, pagedProducts.TotalCount);

            return ApiResponse<PagedResponse<ProductResponse>>.Success(pagedResponse);
        }

         

        

    }
}

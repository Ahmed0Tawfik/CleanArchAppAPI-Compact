using CleanArch.Application;
using CleanArch.Application.APIResponse;
using CleanArch.Application.Auth;
using CleanArch.Application.ProductCQ;
using CleanArch.Application.RequestHandlingService;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CleanArch.API.EndPoints
{
    public static class ProductEndPoints
    {
        public static void MapProductEndPoints(this IEndpointRouteBuilder app)
        {
            var endpoints = app.MapGroup("/Products")
                .WithOpenApi();

            endpoints.MapPost("/add-product", async (
                [FromBody] AddProductCommand request,
                [FromServices] IRequestSender sender, 
                IValidator<AddProductCommand> validator,
                CancellationToken ct) =>
            {
                await validator.ValidateAndThrowAsync(request, ct);
                return await sender.Send(request, ct);
            })
            .WithSummary("Add Product");




            endpoints.MapGet("/get-all", async (
                [FromQuery] int pageNumber,
                [FromQuery] int pageSize,
                [FromQuery] bool? InStock,
                [FromQuery] string? search,
                [FromQuery] bool? IsNew,
                [FromServices] IRequestSender sender,
                IValidator<GetAllProductsRequest> validator,
                CancellationToken ct) =>
            {
                var request = new GetAllProductsRequest();
                request.PageNumber = pageNumber;
                request.PageSize = pageSize;
                request.InStock = InStock;
                request.Search = search;
                request.IsNew = IsNew;
                await validator.ValidateAndThrowAsync(request, ct);
                return await sender.Send(request, ct);
            })
            .WithSummary("Get All Products");

            endpoints.MapDelete("/delete-product", async (
                [FromQuery] Guid Id,
                [FromServices] IRequestSender sender,
                IValidator<DeleteProductCommnad> validator,
                CancellationToken ct) =>
            {
                var request = new DeleteProductCommnad();
                request.Id = Id;
                await validator.ValidateAndThrowAsync(request, ct);
                return await sender.Send(request, ct);
            })
            .WithSummary("Delete Product"); ;

            endpoints.MapPut("/update-product", async (
                [FromBody] UpdateProductCommand request,
                [FromServices] IRequestSender sender,
                IValidator<UpdateProductCommand> validator,
                CancellationToken ct) =>
            {
                await validator.ValidateAndThrowAsync(request, ct);
                return await sender.Send(request, ct);
            })
            .WithSummary("Update Product");


            endpoints.MapPut("/toggle-in-stock", async (
                [FromBody] ToggleInStockProductCommand request,
                [FromServices] IRequestSender sender,
                IValidator<ToggleInStockProductCommand> validator,
                CancellationToken ct) =>
            {
                await validator.ValidateAndThrowAsync(request, ct);
                return await sender.Send(request, ct);
            })
            .WithSummary("Toggle InStock");
           
        }
    }
}

namespace CleanArch.Application.Common.Interfaces
{
    public interface IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>  // Constrain the REQUEST type
    {
        Task<TResponse> Handle(TRequest request, CancellationToken ct = default);
    }

}

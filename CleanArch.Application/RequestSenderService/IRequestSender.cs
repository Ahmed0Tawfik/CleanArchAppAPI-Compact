using CleanArch.Application.Common.Interfaces;

namespace CleanArch.Application.RequestHandlingService
{
    public interface IRequestSender
    {
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct);
    }
}

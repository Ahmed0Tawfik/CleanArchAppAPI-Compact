namespace CleanArch.Application.RequestHandlingService
{
    public class RequestSender : IRequestSender
    {
        private readonly IServiceProvider _serviceProvider;

        public RequestSender(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct)
        {
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var handler = _serviceProvider.GetService(handlerType);

            if (handler == null)
            {
                throw new InvalidOperationException($"Handler for type {handlerType.Name} not registered.");
            }

            var method = handlerType.GetMethod("Handle");
            return (Task<TResponse>)method.Invoke(handler, new object[] { request, ct });
        }
    }
}
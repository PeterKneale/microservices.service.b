using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ServiceB.Api;

public class GrpcLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<GrpcLoggingBehavior<TRequest, TResponse>> _logger;

    public GrpcLoggingBehavior(ILogger<GrpcLoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).FullName;
        var requestJson = JsonConvert.SerializeObject(request, Formatting.Indented);

        _logger.LogInformation("Handling {requestName}\n{requestJson}",requestName, requestJson);

        var sw = Stopwatch.StartNew();
        var response = await next();
        sw.Stop();

        var responseJson = JsonConvert.SerializeObject(response, Formatting.Indented);
        _logger.LogInformation("Handled {requestName} in {ElapsedMilliseconds}ms\n{responseJson}", requestName, sw.ElapsedMilliseconds, responseJson);

        return response;
    }
}
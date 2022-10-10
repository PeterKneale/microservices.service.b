using FluentValidation;
using Grpc.Core;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServiceB.Application.Queries.GetItem;

namespace ServiceB.Api;

public class GrpcValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validations;
    private readonly ILogger<GrpcLoggingBehavior<TRequest, TResponse>> _logger;

    public GrpcValidationBehavior(IEnumerable<IValidator<TRequest>> validations, ILogger<GrpcLoggingBehavior<TRequest, TResponse>> logger)
    {
        _validations = validations;
        _logger = logger;
    }

    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validations.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var failures = _validations
                .Select(v => v.Validate(context))
                .SelectMany(result => result.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count == 0)
            {
                _logger.LogInformation($"Validation of {nameof(Request)} successful");
                return next();
            }
            
            _logger.LogError($"Validation of {nameof(Request)} failed {JsonConvert.SerializeObject(failures)}");
            throw new RpcException(new Status(StatusCode.InvalidArgument, JsonConvert.SerializeObject(failures)));
        }

        _logger.LogWarning($"No validation of {nameof(Request)} performed");

        return next();
    }
}
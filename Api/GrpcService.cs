using Grpc.Core;
using MediatR;
using ServiceB.Application.Queries.GetItem;

namespace ServiceB.Api;

public class GrpcService : ServiceB.ServiceBBase
{
    private readonly IMediator _mediator;

    public GrpcService(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public override async Task<GetItemResponse> GetItem(GetItemRequest request, ServerCallContext context)
    {
        var response = await _mediator.Send(new Request(request.Message));
        return new GetItemResponse
        {
            Message = response.Message
        };
    }
}
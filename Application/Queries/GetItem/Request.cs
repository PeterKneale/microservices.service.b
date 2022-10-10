using MediatR;

namespace ServiceB.Application.Queries.GetItem;

public class Request : IRequest<Response>
{
    public Request(string message)
    {
        Message = message;
    }

    public string Message { get; }
}
using System.Net;
using Chassis.Abstractions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceB.Api;
using ServiceB.Api.Validators;
using ServiceB.Application.Queries.GetItem;

namespace ServiceB;

public class ServiceEntryPoint : IServiceEntryPoint
{
    public void ConfigureWebApplicationBuilder(WebApplicationBuilder builder, IServiceSettings settings)
    {
        // Host
        builder.Configuration
            .AddJsonFile(Path.Combine(settings.ContentRootPath, "settings.json"), optional: false);

        // API
        builder.Services
            .AddGrpc();
        
        builder.Services
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(GrpcLoggingBehavior<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(GrpcValidationBehavior<,>));
        
        builder.Services
            .AddValidatorsFromAssembly(typeof(GetItemValidator).Assembly);
        
        builder.WebHost
            .UseKestrel(opt =>
            {
                var (httpPort, grpcPort) = GetConfiguredPorts();
                // Operate one port in HTTP/1.1 mode for k8s health-checks etc
                opt.Listen(IPAddress.Any, httpPort, listen => listen.Protocols = HttpProtocols.Http1);
                // Operate one port in HTTP/2 mode for GRPC
                opt.Listen(IPAddress.Any, grpcPort, listen => listen.Protocols = HttpProtocols.Http2);
            });

        // Application
        builder.Services.AddMediatR(typeof(Handler).Assembly);
    }

    public void ConfigureWebApplication(WebApplication app)
    {
        app.MapGrpcService<GrpcService>();
    }

    private static (int httpPort, int grpcPort) GetConfiguredPorts()
    {
        var httpPort = int.Parse(Environment.GetEnvironmentVariable("HTTP_PORT") ?? "5000");
        var grpcPort = int.Parse(Environment.GetEnvironmentVariable("GRPC_PORT") ?? "5001");
        return (httpPort, grpcPort);
    }
}
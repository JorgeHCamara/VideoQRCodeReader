using MassTransit;
using VideoQRCodeReader.Worker.Consumers;
using VideoQRCodeReader.Infrastructure.Extensions;
using VideoQRCodeReader.Application.Services;

var builder = Host.CreateApplicationBuilder(args);

// Add Infrastructure services for Worker
builder.Services.AddInfrastructure(builder.Configuration);

// Add Application services
builder.Services.AddScoped<IVideoAnalysisService, VideoAnalysisService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<VideoUploadedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var host = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq";
        var user = builder.Configuration["RabbitMQ:User"] ?? "guest";
        var pass = builder.Configuration["RabbitMQ:Password"] ?? "guest";

        cfg.Host(host, "/", h =>
        {
            h.Username(user);
            h.Password(pass);
        });

        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
await host.RunAsync();

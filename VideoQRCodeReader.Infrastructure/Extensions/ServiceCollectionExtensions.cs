using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;
using VideoQRCodeReader.Infrastructure.Interfaces;
using VideoQRCodeReader.Infrastructure.Services;

namespace VideoQRCodeReader.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register infrastructure services
            services.AddScoped<IFileStorageService, FileStorageService>();
            services.AddScoped<IMessageQueueService, MassTransitQueueService>();
            services.AddScoped<IVideoProcessingService, FFMpegVideoProcessingService>();

            return services;
        }

        public static IServiceCollection AddMassTransitInfrastructure(this IServiceCollection services, IConfiguration configuration, bool includeConsumers = false)
        {
            services.AddMassTransit(x =>
            {
                // Only add consumers if specified (for Worker project)
                if (includeConsumers)
                {
                    // Consumer registration will be handled by the Worker project
                }

                x.UsingRabbitMq((context, cfg) =>
                {
                    var host = configuration["RabbitMQ:Host"] ?? "rabbitmq";
                    var user = configuration["RabbitMQ:User"] ?? "guest";
                    var pass = configuration["RabbitMQ:Password"] ?? "guest";

                    cfg.Host(host, "/", h =>
                    {
                        h.Username(user);
                        h.Password(pass);
                    });

                    if (includeConsumers)
                    {
                        cfg.ConfigureEndpoints(context);
                    }
                });
            });

            return services;
        }
    }
}

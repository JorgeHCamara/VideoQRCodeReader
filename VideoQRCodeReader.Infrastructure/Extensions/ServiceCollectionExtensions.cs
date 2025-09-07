using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;
using MongoDB.Driver;
using VideoQRCodeReader.Infrastructure.Interfaces;
using VideoQRCodeReader.Infrastructure.Services;

namespace VideoQRCodeReader.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register infrastructure services following SOLID principles
            services.AddScoped<IFileStorageService, FileStorageService>();
            services.AddScoped<IMessageQueueService, MassTransitQueueService>();
            
            // Configure MongoDB (required)
            services.AddMongoDB(configuration);
            services.AddScoped<IVideoStatusService, VideoStatusService>();
            services.AddScoped<IVideoResultsService, VideoResultsService>();
            
            // Separate services for video processing and QR code detection (SRP)
            services.AddScoped<IVideoProcessingService, FFMpegVideoProcessingService>();
            services.AddScoped<IQrCodeDetectionService, QrCodeDetectionService>();

            return services;
        }

        public static IServiceCollection AddMongoDB(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MongoDB");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("MongoDB connection string is required. Please configure 'ConnectionStrings:MongoDB' in appsettings.json");
            }

            var databaseName = configuration["MongoDB:DatabaseName"];
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new InvalidOperationException("MongoDB database name is required. Please configure 'MongoDB:DatabaseName' in appsettings.json");
            }

            services.AddSingleton<IMongoClient>(provider =>
            {
                return new MongoClient(connectionString);
            });

            services.AddScoped<IMongoDatabase>(provider =>
            {
                var client = provider.GetRequiredService<IMongoClient>();
                return client.GetDatabase(databaseName);
            });

            return services;
        }

        public static IServiceCollection AddMassTransitInfrastructure(this IServiceCollection services, IConfiguration configuration, bool includeConsumers = false)
        {
            services.AddMassTransit(x =>
            {
                // Add consumers if specified (for Worker project or API with status tracking)
                if (includeConsumers)
                {
                    // Consumer registration will be handled by the calling project
                    // Worker registers VideoUploadedConsumer
                    // API registers ProcessingStatusConsumer and CompletedEventConsumer
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
                        
                        // Add connection retry settings for better resilience
                        h.RequestedConnectionTimeout(TimeSpan.FromSeconds(30));
                    });

                    // Configure message retry policy for connection issues
                    cfg.UseMessageRetry(r => r.Exponential(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(2)));

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

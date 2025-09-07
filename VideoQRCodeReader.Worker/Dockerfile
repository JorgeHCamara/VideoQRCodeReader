# Use the official .NET runtime as a base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files (from solution root context)
COPY ["VideoQRCodeReader.Worker/VideoQRCodeReader.Worker.csproj", "VideoQRCodeReader.Worker/"]
COPY ["VideoQRCodeReader.Application/VideoQRCodeReader.Application.csproj", "VideoQRCodeReader.Application/"]
COPY ["VideoQRCodeReader.Infrastructure/VideoQRCodeReader.Infrastructure.csproj", "VideoQRCodeReader.Infrastructure/"]
COPY ["VideoQRCodeReader.Domain/VideoQRCodeReader.Domain.csproj", "VideoQRCodeReader.Domain/"]
COPY ["VideoQRCodeReader.Contracts/VideoQRCodeReader.Contracts.csproj", "VideoQRCodeReader.Contracts/"]

# Restore dependencies
RUN dotnet restore "VideoQRCodeReader.Worker/VideoQRCodeReader.Worker.csproj"

# Copy source code (from solution root context)
COPY VideoQRCodeReader.Worker/ VideoQRCodeReader.Worker/
COPY VideoQRCodeReader.Application/ VideoQRCodeReader.Application/
COPY VideoQRCodeReader.Infrastructure/ VideoQRCodeReader.Infrastructure/
COPY VideoQRCodeReader.Domain/ VideoQRCodeReader.Domain/
COPY VideoQRCodeReader.Contracts/ VideoQRCodeReader.Contracts/

# Build the application
WORKDIR "/src/VideoQRCodeReader.Worker"
RUN dotnet build "VideoQRCodeReader.Worker.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "VideoQRCodeReader.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app

# Install FFmpeg for video processing
RUN apt-get update && apt-get install -y \
    ffmpeg \
    && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "VideoQRCodeReader.Worker.dll"]

#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy all project files for restore
COPY ["VideoQRCodeReader/VideoQRCodeReader.csproj", "VideoQRCodeReader/"]
COPY ["VideoQRCodeReader.Application/VideoQRCodeReader.Application.csproj", "VideoQRCodeReader.Application/"]
COPY ["VideoQRCodeReader.Infrastructure/VideoQRCodeReader.Infrastructure.csproj", "VideoQRCodeReader.Infrastructure/"]
COPY ["VideoQRCodeReader.Domain/VideoQRCodeReader.Domain.csproj", "VideoQRCodeReader.Domain/"]
COPY ["VideoQRCodeReader.Contracts/VideoQRCodeReader.Contracts.csproj", "VideoQRCodeReader.Contracts/"]

# Restore dependencies
RUN dotnet restore "VideoQRCodeReader/VideoQRCodeReader.csproj"

# Copy all source code
COPY VideoQRCodeReader/ VideoQRCodeReader/
COPY VideoQRCodeReader.Application/ VideoQRCodeReader.Application/
COPY VideoQRCodeReader.Infrastructure/ VideoQRCodeReader.Infrastructure/
COPY VideoQRCodeReader.Domain/ VideoQRCodeReader.Domain/
COPY VideoQRCodeReader.Contracts/ VideoQRCodeReader.Contracts/

WORKDIR "/src/VideoQRCodeReader"
RUN dotnet build "VideoQRCodeReader.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "VideoQRCodeReader.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Set environment variables for Docker container
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VideoQRCodeReader.dll"]
# VideoQRCodeReader - Full Stack Application

A complete video QR code detection system with Angular frontend, .NET Core API, and worker services.

## Architecture

- **Frontend**: Angular 16 application served via Nginx
- **API**: .NET Core Web API for video upload and status tracking
- **Worker**: Background service for video processing and QR code detection
- **Database**: MongoDB for data persistence
- **Message Queue**: RabbitMQ for async communication
- **Admin Tools**: Mongo Express for database administration

## Quick Start

### Prerequisites

- Docker and Docker Compose
- At least 4GB RAM available for containers

### Running the Application

1. Clone the repository:

```bash
git clone <repository-url>
cd VideoQRCodeReader
```

2. Start all services:

```bash
docker-compose up --build
```

3. Wait for all services to start (usually 2-3 minutes on first run)

4. Access the application:
   - **Frontend**: http://localhost:3000
   - **API Documentation**: http://localhost:5000/swagger
   - **RabbitMQ Management**: http://localhost:15672 (guest/guest)
   - **MongoDB Admin**: http://localhost:8081 (admin/admin)

### Services Overview

| Service       | Port       | Description                              |
| ------------- | ---------- | ---------------------------------------- |
| Frontend      | 3000       | Angular application with video upload UI |
| API           | 5000       | REST API for video upload and status     |
| Worker        | -          | Background video processing service      |
| MongoDB       | 27017      | Database storage                         |
| RabbitMQ      | 5672/15672 | Message queue and admin panel            |
| Mongo Express | 8081       | Database administration tool             |

## Using the Application

1. **Upload Video**: Navigate to http://localhost:3000 and drag/drop or select a video file
2. **Processing**: The video will be uploaded and processed in the background
3. **Results**: QR codes detected in the video will be displayed with timestamps and positions
4. **Monitoring**: Check processing status in real-time through the UI

## Development

### Frontend Development

```bash
cd VideoQRCodeReader.Client/video-qr-code-reader-client
npm install
npm start  # Runs on http://localhost:4200
```

### API Development

```bash
cd VideoQRCodeReader
dotnet run  # Runs on https://localhost:7142
```

### Stopping the Application

```bash
docker-compose down
```

### Clearing Data

```bash
docker-compose down -v  # Removes volumes (data will be lost)
```

## Supported Video Formats

- MP4
- AVI
- MOV
- Maximum file size: 100MB

## Environment Variables

Key environment variables can be customized in `docker-compose.yml`:

- `RabbitMQ__Host`: RabbitMQ server hostname
- `ConnectionStrings__MongoDB`: MongoDB connection string
- `MongoDB__DatabaseName`: Database name for the application

## Troubleshooting

### Common Issues

1. **Port Conflicts**: Ensure ports 3000, 5000, 5672, 15672, 27017, and 8081 are available
2. **Memory Issues**: Increase Docker memory allocation if containers fail to start
3. **Build Failures**: Run `docker-compose build --no-cache` to rebuild from scratch

### Logs

```bash
# View all service logs
docker-compose logs

# View specific service logs
docker-compose logs video-qr-frontend
docker-compose logs video-qr-api
docker-compose logs video-qr-worker
```

### Health Checks

- MongoDB: `docker-compose exec mongodb mongosh --eval "db.adminCommand('ping')"`
- RabbitMQ: `docker-compose exec rabbitmq rabbitmq-diagnostics ping`

## Architecture Notes

- The frontend uses nginx as a reverse proxy to route API calls to the backend
- File uploads are stored in a shared volume between API and Worker services
- Processing is asynchronous using RabbitMQ message queues
- MongoDB is used for persistent storage of video metadata and QR code results
- All services include health checks for reliable startup ordering

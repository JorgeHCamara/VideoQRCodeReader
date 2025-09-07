using VideoQRCodeReader.Contracts.DTOs;
using Microsoft.AspNetCore.Http;

namespace VideoQRCodeReader.Application.Services
{
    public interface IVideoUploadService
    {
        Task<VideoUploadResult> ProcessUploadAsync(IFormFile file);
    }
}

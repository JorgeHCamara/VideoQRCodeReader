using Microsoft.AspNetCore.Http;

namespace VideoQRCodeReader.Infrastructure.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, string videoId);
    }
}

namespace VideoQRCodeReader.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, string videoId);
    }
}

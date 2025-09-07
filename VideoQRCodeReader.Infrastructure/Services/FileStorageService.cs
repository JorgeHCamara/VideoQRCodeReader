using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using VideoQRCodeReader.Infrastructure.Interfaces;

namespace VideoQRCodeReader.Infrastructure.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _uploadFolder;

        public FileStorageService(IConfiguration configuration)
        {
            // Pega pasta de uploads do appsettings.json, se não existir usa "uploads"
            _uploadFolder = configuration["Storage:UploadFolder"] ?? "uploads";
        }

        public async Task<string> SaveFileAsync(IFormFile file, string videoId)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Arquivo inválido");

            // Cria a pasta se não existir
            Directory.CreateDirectory(_uploadFolder);

            // Monta o caminho do arquivo
            var safeFileName = Path.GetFileName(file.FileName); // proteção contra path traversal
            var filePath = Path.Combine(_uploadFolder, $"{videoId}_{safeFileName}");

            // Salva o arquivo em disco
            using (var stream = File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            return filePath;
        }
    }
}

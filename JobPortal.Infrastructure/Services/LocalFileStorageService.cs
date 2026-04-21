using JobPortal.Application.Common.Settings;
using JobPortal.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace JobPortal.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageSettings _settings;

    public LocalFileStorageService(IOptions<FileStorageSettings> options)
    {
        _settings = options.Value;
    }

    public async Task<string> SaveResumeAsync(Stream fileStream, string fileName)
    {
        // Ensure the upload folder exists
        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), _settings.UploadPath);
        Directory.CreateDirectory(uploadPath);

        // Generate a unique filename to avoid collisions
        var extension = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadPath, uniqueFileName);

        using var fileOutput = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(fileOutput);

        // Return a relative URL — later Azure will return a blob URL instead
        return $"/{_settings.UploadPath}/{uniqueFileName}";
    }

    public Task DeleteAsync(string fileUrl)
    {
        // Convert URL back to physical path
        var relativePath = fileUrl.TrimStart('/');
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }
}

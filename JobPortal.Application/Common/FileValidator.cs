using JobPortal.Application.Common.Exceptions;
using JobPortal.Application.Common.Settings;
using Microsoft.Extensions.Options;

namespace JobPortal.Application.Common;

public class FileValidator
{
    private readonly FileStorageSettings _settings;

    public FileValidator(IOptions<FileStorageSettings> options)
    {
        _settings = options.Value;
    }

    public void Validate(string fileName, long fileSizeBytes)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (!_settings.AllowedExtensions.Contains(extension))
            throw new AppException(
                $"File type not allowed. Allowed types: {string.Join(", ", _settings.AllowedExtensions)}");

        if (fileSizeBytes > _settings.MaxFileSizeBytes)
            throw new AppException(
                $"File size exceeds the {_settings.MaxFileSizeBytes / 1024 / 1024}MB limit.");
    }
}

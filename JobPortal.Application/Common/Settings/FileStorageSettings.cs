using System;

namespace JobPortal.Application.Common.Settings;

public class FileStorageSettings
{
    public string UploadPath { get; set; } = null!;   // physical folder on disk

    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;  // 5MB default
    
    public string[] AllowedExtensions { get; set; } = { ".pdf", ".doc", ".docx" };
}

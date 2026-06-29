using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using JobPortal.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace JobPortal.Infrastructure.Services;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureBlobStorage:ConnectionString"];
        var containerName = configuration["AzureBlobStorage:ContainerName"];

        var blobServiceClient = new BlobServiceClient(connectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
    }

    public async Task<string> SaveResumeAsync(Stream fileStream, string fileName)
    {
        // Generate a unique blob name to avoid overwrites
        var blobName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";

        BlobClient blobClient = _containerClient.GetBlobClient(blobName);

        await blobClient.UploadAsync(fileStream, new BlobHttpHeaders
        {
            ContentType = "application/pdf"
        });

        return blobClient.Uri.ToString();// Returns the full public URL of the uploaded file
    }

    public async Task DeleteAsync(string fileUrl)
    {
        // Extract blob name from the full URL
        var uri = new Uri(fileUrl);
        var blobName = Path.GetFileName(uri.LocalPath);

        BlobClient blobClient = _containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync();
    }
}
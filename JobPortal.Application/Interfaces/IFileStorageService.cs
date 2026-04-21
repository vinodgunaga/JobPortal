namespace JobPortal.Application.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Saves the file and returns the URL to access it.
    /// </summary>
    Task<string> SaveResumeAsync(Stream fileStream, string fileName);

    /// <summary>
    /// Deletes a file by its URL.
    /// </summary>
    Task DeleteAsync(string fileUrl);
}

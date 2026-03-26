namespace ConstructionPayment.Application.Interfaces;

public interface IFileStorageService
{
    Task<(string storedFileName, string filePath)> SaveFileAsync(Stream stream, string originalFileName, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(string filePath, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
}

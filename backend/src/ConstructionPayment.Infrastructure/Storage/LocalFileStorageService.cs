using ConstructionPayment.Application.Exceptions;
using ConstructionPayment.Application.Interfaces;
using ConstructionPayment.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace ConstructionPayment.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService(IOptions<StorageOptions> options)
    {
        var uploadPath = options.Value.UploadPath;
        _rootPath = Path.GetFullPath(uploadPath);
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<(string storedFileName, string filePath)> SaveFileAsync(Stream stream, string originalFileName, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(originalFileName);
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var absoluteFilePath = Path.Combine(_rootPath, storedFileName);

        await using (var fileStream = new FileStream(absoluteFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await stream.CopyToAsync(fileStream, cancellationToken);
        }

        return (storedFileName, absoluteFilePath);
    }

    public Task<Stream> OpenReadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            throw new NotFoundException("Không tìm thấy file lưu trữ.");
        }

        Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }
}

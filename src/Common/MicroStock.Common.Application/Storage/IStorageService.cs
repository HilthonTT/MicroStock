namespace MicroStock.Common.Application.Storage;

public interface IStorageService
{
    Task<Uri> UploadAsync<T>(
        string name, 
        string extension, 
        string data, 
        FileType supportedFileType, 
        CancellationToken cancellationToken = default);

    public void Remove(Uri? path);
}

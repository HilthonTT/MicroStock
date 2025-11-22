namespace MicroStock.Common.Application.ETag;

public interface IETagStore
{
    string GetETag(string resourceUri);

    void SetETag(string resourceUri, string etag);

    void SetETag(string resourceUri, object resource);

    void RemoveETag(string resourceUri);
}

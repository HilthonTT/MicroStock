namespace Application.Abstractions.Email;

public sealed record EmailAttachment
{
    public string FileName { get; private set; }

    public byte[] Content { get; private set; }

    public string ContentType { get; private set; } = "application/octet-stream";

    public EmailAttachment(string filePath)
    {
        FileName = Path.GetFileName(filePath);
        Content = File.ReadAllBytes(filePath);
        ContentType = GetContentType(filePath);
    }

    private static string GetContentType(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".txt" => "text/plain",
            ".html" => "text/html",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }
}
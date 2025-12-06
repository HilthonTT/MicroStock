namespace Application.Abstractions.Email;

public sealed record EmailRequest
{
    public List<string> To { get; set; } = [];

    public List<string> Cc { get; set; } = [];

    public List<string> Bcc { get; set; } = [];

    public string Subject { get; set; } = string.Empty; 

    public string Body { get; set; } = string.Empty; 

    public bool IsHtml { get; set; }

    public List<EmailAttachment> Attachments { get; set; } = [];
    public Dictionary<string, string> Headers { get; set; } = [];
}

namespace HtmlProcessor.Api.Models;

public sealed class ProcessRequest
{
    public string? Selector { get; set; }
    public string? Attribute { get; set; }
    public string? UrlB64 { get; set; }
    public string? EncryptedTextBytesB64 { get; set; }
    public string? KeyBytesB64 { get; set; }
    public string? PageB64 { get; set; }
}
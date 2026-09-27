namespace HtmlProcessor.Api.Models;

public sealed class ElementEntity
{
    public long Id { get; set; }
    public string? AttributeValue { get; set; }
    public string HtmlContent { get; set; } = string.Empty;
}
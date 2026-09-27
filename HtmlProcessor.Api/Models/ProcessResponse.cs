namespace HtmlProcessor.Api.Models;

public sealed class ProcessResponse
{
    public int IsError { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public int ElementsCount { get; set; }
    public int EmailsCount { get; set; }
    public string? Url { get; set; }
    public string? DecryptedPlainText { get; set; }
    public List<string> ElementsAttrList { get; set; } = [];
    public List<string> EmailsList { get; set; } = [];
}
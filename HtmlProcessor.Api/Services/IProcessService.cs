using HtmlProcessor.Api.Models;

namespace HtmlProcessor.Api.Services;

public interface IProcessService
{
    Task<ProcessResponse> ProcessAsync(ProcessRequest request, CancellationToken cancellationToken = default);
}
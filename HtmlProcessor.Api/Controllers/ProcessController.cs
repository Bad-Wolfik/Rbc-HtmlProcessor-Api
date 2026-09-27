using HtmlProcessor.Api.Models;
using HtmlProcessor.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HtmlProcessor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProcessController : ControllerBase
{
    private readonly IProcessService _processService;

    public ProcessController(IProcessService processService)
    {
        _processService = processService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProcessResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Process([FromBody] ProcessRequest request, CancellationToken cancellationToken)
    {
        var response = await _processService.ProcessAsync(request, cancellationToken);
        return Ok(response);
    }
}
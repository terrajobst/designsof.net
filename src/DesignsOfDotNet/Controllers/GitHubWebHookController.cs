using DesignsOfDotNet.Data;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DesignsOfDotNet.Controllers;

[ApiController]
[Route("github-webhook")]
[AllowAnonymous]
public sealed class GitHubWebHookController : Controller
{
    private readonly DesignService _designService;

    public GitHubWebHookController(DesignService designService)
    {
        _designService = designService;
    }

    [HttpPost]
    public IActionResult Post()
    {
        // Don't await. Just kick of the work here so we don't time out.
        _ = _designService.UpdateAsync();

        return Ok();
    }
}
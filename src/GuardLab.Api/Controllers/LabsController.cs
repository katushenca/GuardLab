using GuardLab.Application.Labs;
using GuardLab.Application.Labs.IdorKeyAccess;
using Microsoft.AspNetCore.Mvc;

namespace GuardLab.Api.Controllers;

[ApiController]
[Route("api/labs")]
public sealed class LabsController(LabCatalogService catalog, LabRunService runs) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LabSummaryResponse>>> List(CancellationToken cancellationToken)
    {
        var labs = await catalog.GetPublishedAsync(cancellationToken);
        return Ok(labs.Select(LabSummaryResponse.From).ToArray());
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<LabDetailsResponse>> Get(string slug, CancellationToken cancellationToken)
    {
        var lab = await catalog.GetBySlugAsync(slug, cancellationToken);
        return lab is null ? NotFound() : Ok(LabDetailsResponse.From(lab));
    }

    [HttpPost("{slug}/run")]
    public async Task<ActionResult<LabRunResult>> Run(string slug, [FromBody] RunLabRequest request, CancellationToken cancellationToken)
    {
        var lab = await catalog.GetBySlugAsync(slug, cancellationToken);
        if (lab is null) return NotFound();
        try
        {
            return Ok(await runs.RunAsync(lab.Id, request.Sid, request.KeyId, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }
}

public sealed record RunLabRequest(string Sid, Guid KeyId);

using GuardLab.Application.Labs;
using GuardLab.Core.Labs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GuardLab.Api.Controllers;

[ApiController]
[Route("api/admin/labs")]
[Authorize(Roles = "admin")]
public sealed class AdminLabsController(LabAdminService labs, LabRunService runs) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok((await labs.GetAllAsync(cancellationToken)).Select(AdminLabResponse.From));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var lab = await labs.GetByIdAsync(id, cancellationToken);
        return lab is null ? NotFound() : Ok(AdminLabResponse.From(lab));
    }

    [HttpPost("{id:guid}/run")]
    public async Task<IActionResult> Preview(Guid id, [FromBody] RunLabRequest request,
        CancellationToken cancellationToken)
    {
        var lab = await labs.GetByIdAsync(id, cancellationToken);
        if (lab is null) return NotFound();
        if (lab.Slug != "idor-key-access")
            return Conflict(new
            {
                error = "scenario_not_implemented",
                message = "Учебный обработчик для этой лаборатории ещё не подключён."
            });
        try
        {
            return Ok(await runs.RunAsync(id, request.Sid, request.KeyId, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "invalid_training_input", message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveLabRequest request, CancellationToken cancellationToken)
    {
        var lab = new Lab(request.Slug, request.Title, request.Summary, request.Theory);
        if (request.Status == LabStatus.Archived)
            return Conflict(
                new { error = "invalid_status_transition", message = "A new lab cannot start as Archived." });
        if (request.Status == LabStatus.Published) lab.Publish();
        try
        {
            var created = await labs.CreateAsync(lab, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, AdminLabResponse.From(created));
        }
        catch (DuplicateLabSlugException ex)
        {
            return Conflict(new { error = "slug_already_exists", message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaveLabRequest request,
        CancellationToken cancellationToken)
    {
        var lab = await labs.GetByIdAsync(id, cancellationToken);
        if (lab is null) return NotFound();
        lab.UpdateContent(request.Slug, request.Title, request.Summary, request.Theory);
        try
        {
            lab.TransitionTo(request.Status);
            await labs.UpdateAsync(lab, cancellationToken);
            return Ok(AdminLabResponse.From(lab));
        }
        catch (DuplicateLabSlugException ex)
        {
            return Conflict(new { error = "slug_already_exists", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = "invalid_status_transition", message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return await labs.ArchiveAsync(id, cancellationToken) ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = "invalid_status_transition", message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return await labs.RestoreAsync(id, cancellationToken) ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = "invalid_status_transition", message = ex.Message });
        }
    }
}

public sealed record SaveLabRequest(
    [Required, StringLength(100, MinimumLength = 2)]
    string Slug,
    [Required, StringLength(200, MinimumLength = 2)]
    string Title,
    [Required, StringLength(1000, MinimumLength = 2)]
    string Summary,
    [Required, MinLength(2)] string Theory,
    LabStatus Status = LabStatus.Draft);

public sealed record AdminLabResponse(
    Guid Id,
    string Slug,
    string Title,
    string Summary,
    string Theory,
    LabStatus Status)
{
    public static AdminLabResponse From(Lab lab) =>
        new(lab.Id, lab.Slug, lab.Title, lab.Summary, lab.Theory, lab.Status);
}
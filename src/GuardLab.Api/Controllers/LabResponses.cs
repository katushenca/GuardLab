using GuardLab.Core.Labs;

namespace GuardLab.Api.Controllers;

public sealed record LabSummaryResponse(Guid Id, string Slug, string Title, string Summary)
{
    public static LabSummaryResponse From(Lab lab) => new(lab.Id, lab.Slug, lab.Title, lab.Summary);
}

public sealed record LabDetailsResponse(Guid Id, string Slug, string Title, string Summary, string Theory)
{
    public static LabDetailsResponse From(Lab lab) => new(lab.Id, lab.Slug, lab.Title, lab.Summary, lab.Theory);
}

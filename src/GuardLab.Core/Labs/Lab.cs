namespace GuardLab.Core.Labs;

public sealed class Lab
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Slug { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public string Theory { get; private set; } = string.Empty;
    public LabStatus Status { get; private set; } = LabStatus.Draft;

    private Lab() { }

    public Lab(string slug, string title, string summary, string theory)
    {
        Slug = slug;
        Title = title;
        Summary = summary;
        Theory = theory;
    }

    public static Lab Rehydrate(Guid id, string slug, string title, string summary, string theory, LabStatus status)
    {
        var lab = new Lab(slug, title, summary, theory) { Id = id, Status = status };
        return lab;
    }

    public void Publish() => Status = LabStatus.Published;

    public void Archive() => Status = LabStatus.Archived;

    public void TransitionTo(LabStatus status)
    {
        if (status == Status) return;
        if (Status == LabStatus.Draft && status == LabStatus.Published)
        {
            Publish();
            return;
        }
        if (Status == LabStatus.Published && status == LabStatus.Archived)
        {
            Archive();
            return;
        }
        if (Status == LabStatus.Archived && status == LabStatus.Draft)
        {
            Status = LabStatus.Draft;
            return;
        }
        if (Status == LabStatus.Archived && status == LabStatus.Draft)
        {
            Status = LabStatus.Draft;
            return;
        }
        throw new InvalidOperationException($"Invalid lab status transition: {Status} -> {status}.");
    }

    public void UpdateContent(string slug, string title, string summary, string theory)
    {
        Slug = slug;
        Title = title;
        Summary = summary;
        Theory = theory;
    }
}

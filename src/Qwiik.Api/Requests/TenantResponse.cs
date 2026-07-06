namespace Qwiik.Api.Requests;

public sealed class TenantResponse
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Slug { get; set; }
}

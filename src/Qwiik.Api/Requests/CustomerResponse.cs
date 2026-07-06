namespace Qwiik.Api.Requests;

public sealed class CustomerResponse
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public required string Name { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? BillingAddress { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}

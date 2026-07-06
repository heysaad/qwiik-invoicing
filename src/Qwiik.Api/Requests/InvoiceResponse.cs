using Qwiik.Api.Data.Models;

namespace Qwiik.Api.Requests;

public sealed class InvoiceResponse
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public required string InvoiceNumber { get; set; }

    public CustomerResponse? Customer { get; set; }

    public DateOnly IssueDate { get; set; }

    public DateOnly? DueDate { get; set; }

    public decimal Subtotal { get; set; }

    public decimal Tax { get; set; }

    public decimal Total { get; set; }

    public InvoiceStatus Status { get; set; }

    public string? Notes { get; set; }

    public IReadOnlyList<InvoiceItemResponse> Items { get; set; } = [];

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}

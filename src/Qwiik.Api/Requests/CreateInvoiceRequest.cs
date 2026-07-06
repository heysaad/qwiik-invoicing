using System.ComponentModel.DataAnnotations;
using Qwiik.Api.Data.Models;

namespace Qwiik.Api.Requests;

public sealed class CreateInvoiceRequest
{
    [Required]
    [MaxLength(64)]
    public required string InvoiceNumber { get; set; }

    public Guid CustomerId { get; set; }

    public DateOnly IssueDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public DateOnly? DueDate { get; set; }

    [Range(0, 999999999999.99)]
    public decimal Tax { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    [MaxLength(1024)]
    public string? Notes { get; set; }

    public List<InvoiceItemRequest> Items { get; set; } = [];
}

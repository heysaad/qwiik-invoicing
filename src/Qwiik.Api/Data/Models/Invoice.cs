using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Qwiik.Api.Data.Models;

[Index(nameof(TenantId), nameof(InvoiceNumber), IsUnique = true)]
[Index(nameof(TenantId), nameof(IssueDate))]
public class Invoice
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    [ForeignKey(nameof(TenantId))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Tenant? Tenant { get; set; }

    [Required]
    [MaxLength(64)]
    public required string InvoiceNumber { get; set; }

    public Guid CustomerId { get; set; }

    [ForeignKey(nameof(CustomerId))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Customer? Customer { get; set; }

    public DateOnly IssueDate { get; set; }

    public DateOnly? DueDate { get; set; }

    [Precision(18, 2)]
    public decimal Subtotal { get; set; }

    [Precision(18, 2)]
    public decimal Tax { get; set; }

    [Precision(18, 2)]
    public decimal Total { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    [MaxLength(1024)]
    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<InvoiceItem> Items { get; set; } = [];
}

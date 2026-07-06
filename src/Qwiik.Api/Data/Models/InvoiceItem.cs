using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Qwiik.Api.Data.Models;

[Index(nameof(TenantId), nameof(InvoiceId))]
public class InvoiceItem
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    [ForeignKey(nameof(TenantId))]
    public Tenant? Tenant { get; set; }

    public Guid InvoiceId { get; set; }

    [ForeignKey(nameof(InvoiceId))]
    public Invoice? Invoice { get; set; }

    public int Position { get; set; }

    [Required]
    [MaxLength(256)]
    public required string Description { get; set; }

    [Precision(18, 2)]
    public decimal Quantity { get; set; }

    [Precision(18, 2)]
    public decimal UnitPrice { get; set; }

    [Precision(18, 2)]
    public decimal LineTotal { get; set; }
}

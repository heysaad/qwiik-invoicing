using System.ComponentModel.DataAnnotations;

namespace Qwiik.Api.Requests;

public sealed class InvoiceItemRequest
{
    [Required]
    [MaxLength(256)]
    public required string Description { get; set; }

    [Range(0.01, 999999999999.99)]
    public decimal Quantity { get; set; }

    [Range(0, 999999999999.99)]
    public decimal UnitPrice { get; set; }
}

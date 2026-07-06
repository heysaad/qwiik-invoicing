namespace Qwiik.Api.Requests;

public sealed class InvoiceItemResponse
{
    public Guid Id { get; set; }

    public int Position { get; set; }

    public required string Description { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }
}

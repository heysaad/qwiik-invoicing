using System.ComponentModel.DataAnnotations;

namespace Qwiik.Api.Requests;

public sealed class CreateCustomerRequest
{
    [Required]
    [MaxLength(256)]
    public required string Name { get; set; }

    [EmailAddress]
    [MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(32)]
    public string? Phone { get; set; }

    [MaxLength(512)]
    public string? BillingAddress { get; set; }
}

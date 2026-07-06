using System.ComponentModel.DataAnnotations;

namespace Qwiik.Api.Requests;

public sealed class RegisterTenantRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string Password { get; init; }
}

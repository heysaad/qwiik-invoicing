using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Qwiik.Api.Data.Models;

[Index(nameof(Slug), IsUnique = true)]
public class Tenant
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(256)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(128)]
    public required string Slug { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<ApplicationUser> Users { get; set; } = [];
}

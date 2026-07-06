using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Qwiik.Api.Data.Models;

public class ApplicationUser : IdentityUser
{
    public Guid? TenantId { get; set; }

    [ForeignKey(nameof(TenantId))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Tenant? Tenant { get; set; }
}

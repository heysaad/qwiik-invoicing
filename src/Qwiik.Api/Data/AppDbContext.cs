using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Qwiik.Api.Data.Models;

namespace Qwiik.Api.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
}

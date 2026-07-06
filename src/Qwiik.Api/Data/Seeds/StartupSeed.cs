using Microsoft.EntityFrameworkCore;
using Qwiik.Api.Data.Models;

namespace Qwiik.Api.Data.Seeds;

public class StartupSeed(AppDbContext db, ILogger<StartupSeed> logger)
{
    public async Task RunAsync()
    {
        if (await db.Tenants.AnyAsync())
        {
            return;
        }

        db.Tenants.Add(new Tenant
        {
            Name = "Default",
            Slug = "default"
        });

        await db.SaveChangesAsync();
        logger.LogInformation("Seeded default tenant.");
    }
}

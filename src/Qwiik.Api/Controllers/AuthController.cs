using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Qwiik.Api.Data;
using Qwiik.Api.Data.Models;
using Qwiik.Api.Requests;

namespace Qwiik.Api.Controllers;

[ApiController]
public class AuthController(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("/auth/register")]
    public async Task<IResult> Register(RegisterTenantRequest request)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();

        var tenant = new Tenant
        {
            Name = CreateTenantName(request.Email),
            Slug = await CreateUniqueTenantSlug(request.Email)
        };

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        logger.LogInformation("Registering user for new tenant {TenantId}", tenant.Id);

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            TenantId = tenant.Id
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            await transaction.RollbackAsync();
            return Results.ValidationProblem(result.Errors
                .GroupBy(error => error.Code)
                .ToDictionary(
                    group => group.Key,
                group => group.Select(error => error.Description).ToArray()));
        }

        await transaction.CommitAsync();

        logger.LogInformation("Registered user {UserId} for tenant {TenantId}", user.Id, tenant.Id);
        return Results.Created($"/users/{user.Id}", new
        {
            TenantId = tenant.Id,
            UserId = user.Id
        });
    }

    [Authorize]
    [HttpGet("/me/tenant")]
    public async Task<IResult> GetMyTenant()
    {
        var user = await userManager.GetUserAsync(User);
        if (user?.TenantId is null)
            return Results.NotFound(new { message = "User is not assigned to a tenant." });

        var tenant = await db.Tenants
            .AsNoTracking()
            .Where(tenant => tenant.Id == user.TenantId && tenant.IsActive)
            .Select(tenant => new TenantResponse
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Slug = tenant.Slug
            })
            .SingleOrDefaultAsync();

        return tenant is null
            ? Results.NotFound(new { message = "Tenant does not exist or is inactive." })
            : Results.Ok(tenant);
    }

    private static string CreateTenantName(string email)
    {
        var name = email.Split('@', 2)[0].Trim();
        return string.IsNullOrWhiteSpace(name) ? "Tenant" : name;
    }

    private async Task<string> CreateUniqueTenantSlug(string email)
    {
        var slugBase = new string(CreateTenantName(email)
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray())
            .Trim('-');

        if (string.IsNullOrWhiteSpace(slugBase))
            slugBase = "tenant";

        slugBase = slugBase.Length > 80 ? slugBase[..80] : slugBase;
        var slug = slugBase;
        var index = 2;

        while (await db.Tenants.AnyAsync(tenant => tenant.Slug == slug))
        {
            slug = $"{slugBase}-{index}";
            index++;
        }

        return slug;
    }
}

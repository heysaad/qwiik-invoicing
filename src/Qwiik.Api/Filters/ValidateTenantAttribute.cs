using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Qwiik.Api.Data;
using Qwiik.Api.Extensions;

namespace Qwiik.Api.Filters;

public sealed class ValidateTenantAttribute 
    : TypeFilterAttribute<ValidateTenantFilter>
{
}

public sealed class ValidateTenantFilter(AppDbContext db) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var tenantId = context.HttpContext.Request.GetTenantId();
        if (tenantId is null || tenantId == Guid.Empty)
        {
            context.Result = CreateValidationResult("Tenant id header is required.");
            return;
        }

        var tenantExists = await db.Tenants.AnyAsync(tenant => tenant.Id == tenantId.Value && tenant.IsActive);
        if (!tenantExists)
        {
            context.Result = CreateValidationResult("Tenant does not exist or is inactive.");
            return;
        }

        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated == true)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var isTenantMember = await db.Users.AnyAsync(dbUser =>
                dbUser.Id == userId &&
                dbUser.TenantId == tenantId.Value);

            if (!isTenantMember)
            {
                context.Result = new ForbidResult();
                return;
            }
        }

        await next();
    }

    private static BadRequestObjectResult CreateValidationResult(string error)
    {
        return new BadRequestObjectResult(new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            [RequestExtensions.TenantIdHeaderName] = [error]
        }));
    }
}

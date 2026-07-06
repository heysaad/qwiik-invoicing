using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Qwiik.Api.Data.Models;
using Qwiik.Api.Extensions;
using Qwiik.Api.Filters;
using Qwiik.Api.Requests;

namespace Qwiik.Api.Controllers;

[ApiController]
public class AuthController(UserManager<ApplicationUser> userManager, ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("/register", Name = "RegisterTenant", Order = -1)]
    [ValidateTenant]
    public async Task<IResult> Register(RegisterTenantRequest request)
    {
        var tenantId = Request.GetTenantId()!.Value;
        logger.LogInformation("Registering user for tenant {TenantId}", tenantId);

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            TenantId = tenantId
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return Results.ValidationProblem(result.Errors
                .GroupBy(error => error.Code)
                .ToDictionary(
                    group => group.Key,
                group => group.Select(error => error.Description).ToArray()));
        }

        logger.LogInformation("Registered user {UserId} for tenant {TenantId}", user.Id, tenantId);
        return Results.Created($"/users/{user.Id}", new
        {
            TenantId = tenantId,
            UserId = user.Id
        });
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Qwiik.Api.Data;
using Qwiik.Api.Data.Models;
using Qwiik.Api.Data.Seeds;
using Qwiik.Api.Mapping;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

MapsterConfig.Register();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});
builder.Services.AddHealthChecks();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(IdentityConstants.BearerScheme)
    .AddBearerToken(IdentityConstants.BearerScheme);

builder.Services.AddAuthorizationBuilder();

builder.Services.AddIdentityCore<ApplicationUser>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddApiEndpoints();

builder.Services.AddTransient<StartupSeed>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();

    if(context.Database.GetPendingMigrations().Any())
    {
        app.Logger.LogInformation("Applying database migrations.");
        context.Database.Migrate();
    }

    var seed = services.GetRequiredService<StartupSeed>();
    app.Logger.LogInformation("Running startup seed.");
    await seed.RunAsync();
    app.Logger.LogInformation("Startup seed completed.");
}

app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

app.MapIdentityApi<ApplicationUser>();

app.MapHealthChecks("/health");
app.MapControllers();

app.Logger.LogInformation("Starting API.");
app.Run();

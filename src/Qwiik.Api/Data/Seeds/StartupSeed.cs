using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Qwiik.Api.Data.Models;

namespace Qwiik.Api.Data.Seeds;

public class StartupSeed(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    ILogger<StartupSeed> logger)
{
    private const string DemoTenantSlug = "demo";
    private const string DemoTenantName = "Qwiik Demo";
    private const string DemoUserEmail = "demo@qwiik.local";
    private const string DemoUserPassword = "Demo123!";

    public async Task RunAsync()
    {
        var tenant = await db.Tenants.SingleOrDefaultAsync(tenant => tenant.Slug == DemoTenantSlug);
        if (tenant is not null) return;

        tenant = new Tenant
        {
            Name = DemoTenantName,
            Slug = DemoTenantSlug
        };

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();
        logger.LogInformation("Seeded demo tenant {TenantId}.", tenant.Id);

        await SeedUserAsync(tenant);
        var customers = await SeedCustomersAsync(tenant.Id);
        await SeedInvoicesAsync(tenant.Id, customers);
    }

    private async Task SeedUserAsync(Tenant tenant)
    {
        var user = await userManager.FindByEmailAsync(DemoUserEmail);
        if (user is not null)
        {
            if (user.TenantId != tenant.Id)
            {
                user.TenantId = tenant.Id;
                await userManager.UpdateAsync(user);
            }

            return;
        }

        user = new ApplicationUser
        {
            UserName = DemoUserEmail,
            Email = DemoUserEmail,
            EmailConfirmed = true,
            TenantId = tenant.Id
        };

        var result = await userManager.CreateAsync(user, DemoUserPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Failed to seed demo user: {errors}");
        }

        logger.LogInformation("Seeded demo user {UserId} with email {Email}.", user.Id, DemoUserEmail);
    }

    private async Task<Dictionary<string, Customer>> SeedCustomersAsync(Guid tenantId)
    {
        var existingCustomers = await db.Customers
            .Where(customer => customer.TenantId == tenantId)
            .ToDictionaryAsync(customer => customer.Email ?? customer.Name);

        var customerSeeds = new[]
        {
            new CustomerSeed(
                "Northstar Studio",
                "billing@northstar.example",
                "+1 415 555 0132",
                "450 Market Street, Suite 1800, San Francisco, CA 94105"),
            new CustomerSeed(
                "Bluebird Logistics",
                "accounts@bluebird.example",
                "+1 312 555 0194",
                "210 W Lake Street, Chicago, IL 60606"),
            new CustomerSeed(
                "Evergreen Health Co.",
                "finance@evergreen.example",
                "+1 206 555 0178",
                "88 Pine Avenue, Seattle, WA 98101")
        };

        foreach (var seed in customerSeeds)
        {
            if (existingCustomers.ContainsKey(seed.Email))
                continue;

            var customer = new Customer
            {
                TenantId = tenantId,
                Name = seed.Name,
                Email = seed.Email,
                Phone = seed.Phone,
                BillingAddress = seed.BillingAddress
            };

            db.Customers.Add(customer);
            existingCustomers.Add(seed.Email, customer);
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Seeded demo customers for tenant {TenantId}.", tenantId);

        return existingCustomers;
    }

    private async Task SeedInvoicesAsync(Guid tenantId, IReadOnlyDictionary<string, Customer> customers)
    {
        var existingInvoiceNumbers = await db.Invoices
            .Where(invoice => invoice.TenantId == tenantId)
            .Select(invoice => invoice.InvoiceNumber)
            .ToListAsync();

        var existingInvoiceNumberSet = existingInvoiceNumbers.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var invoiceSeeds = new[]
        {
            new InvoiceSeed(
                "QWK-2026-001",
                "billing@northstar.example",
                new DateOnly(2026, 7, 1),
                new DateOnly(2026, 7, 15),
                InvoiceStatus.Sent,
                "Thanks for choosing Qwiik. Payment is due within 14 days.",
                [
                    new InvoiceItemSeed("Brand audit workshop", 1, 850),
                    new InvoiceItemSeed("Invoice workflow setup", 1, 1250)
                ],
                210),
            new InvoiceSeed(
                "QWK-2026-002",
                "accounts@bluebird.example",
                new DateOnly(2026, 6, 20),
                new DateOnly(2026, 7, 4),
                InvoiceStatus.Overdue,
                "Includes implementation support and rollout review.",
                [
                    new InvoiceItemSeed("Operations dashboard configuration", 1, 1800),
                    new InvoiceItemSeed("Team onboarding session", 2, 300)
                ],
                240),
            new InvoiceSeed(
                "QWK-2026-003",
                "finance@evergreen.example",
                new DateOnly(2026, 6, 5),
                new DateOnly(2026, 6, 19),
                InvoiceStatus.Paid,
                "Paid by bank transfer.",
                [
                    new InvoiceItemSeed("Monthly platform subscription", 1, 499),
                    new InvoiceItemSeed("Priority support package", 1, 350)
                ],
                84.90m),
            new InvoiceSeed(
                "QWK-2026-004",
                "billing@northstar.example",
                new DateOnly(2026, 7, 6),
                new DateOnly(2026, 7, 20),
                InvoiceStatus.Draft,
                "Draft invoice for review before sending.",
                [
                    new InvoiceItemSeed("Creative operations consultation", 3, 275),
                    new InvoiceItemSeed("Custom report template", 1, 425)
                ],
                125)
        };

        foreach (var seed in invoiceSeeds)
        {
            if (existingInvoiceNumberSet.Contains(seed.InvoiceNumber))
                continue;

            if (!customers.TryGetValue(seed.CustomerEmail, out var customer))
                continue;

            var items = seed.Items
                .Select((item, index) => new InvoiceItem
                {
                    TenantId = tenantId,
                    Position = index + 1,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    LineTotal = item.Quantity * item.UnitPrice
                })
                .ToList();

            var subtotal = items.Sum(item => item.LineTotal);
            var invoice = new Invoice
            {
                TenantId = tenantId,
                CustomerId = customer.Id,
                InvoiceNumber = seed.InvoiceNumber,
                IssueDate = seed.IssueDate,
                DueDate = seed.DueDate,
                Subtotal = subtotal,
                Tax = seed.Tax,
                Total = subtotal + seed.Tax,
                Status = seed.Status,
                Notes = seed.Notes,
                Items = items
            };

            db.Invoices.Add(invoice);
            existingInvoiceNumberSet.Add(seed.InvoiceNumber);
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Seeded demo invoices for tenant {TenantId}.", tenantId);
    }

    private sealed record CustomerSeed(string Name, string Email, string Phone, string BillingAddress);

    private sealed record InvoiceSeed(
        string InvoiceNumber,
        string CustomerEmail,
        DateOnly IssueDate,
        DateOnly DueDate,
        InvoiceStatus Status,
        string Notes,
        IReadOnlyList<InvoiceItemSeed> Items,
        decimal Tax);

    private sealed record InvoiceItemSeed(string Description, decimal Quantity, decimal UnitPrice);
}

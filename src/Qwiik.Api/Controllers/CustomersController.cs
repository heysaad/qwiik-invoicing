using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Qwiik.Api.Data;
using Qwiik.Api.Data.Models;
using Qwiik.Api.Extensions;
using Qwiik.Api.Filters;
using Qwiik.Api.Requests;

namespace Qwiik.Api.Controllers;

[ApiController]
[Route("customers")]
[ValidateTenant]
public class CustomersController(AppDbContext db) : ControllerBase
{
    private const int MaxPageSize = 100;

    [HttpGet]
    public async Task<IResult> GetCustomers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var tenantId = Request.GetTenantId()!.Value;
        var query = db.Customers
            .AsNoTracking()
            .Where(customer => customer.TenantId == tenantId)
            .OrderBy(customer => customer.Name)
            .ThenByDescending(customer => customer.CreatedAt);

        var totalCount = await query.CountAsync();
        var customers = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        return Results.Ok(new PagedResponse<CustomerResponse>
        {
            Items = customers.Adapt<List<CustomerResponse>>(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IResult> GetCustomer(Guid id)
    {
        var tenantId = Request.GetTenantId()!.Value;
        var customer = await db.Customers
            .AsNoTracking()
            .SingleOrDefaultAsync(customer => customer.Id == id && customer.TenantId == tenantId);

        return customer is null
            ? Results.NotFound()
            : Results.Ok(customer.Adapt<CustomerResponse>());
    }

    [HttpPost]
    public async Task<IResult> CreateCustomer(CreateCustomerRequest request)
    {
        var tenantId = Request.GetTenantId()!.Value;
        var customer = new Customer
        {
            TenantId = tenantId,
            Name = request.Name.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            BillingAddress = string.IsNullOrWhiteSpace(request.BillingAddress) ? null : request.BillingAddress.Trim()
        };

        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        return Results.Created($"/customers/{customer.Id}", customer.Adapt<CustomerResponse>());
    }

    [HttpPut("{id:guid}")]
    public async Task<IResult> UpdateCustomer(Guid id, UpdateCustomerRequest request)
    {
        var tenantId = Request.GetTenantId()!.Value;
        var customer = await db.Customers.SingleOrDefaultAsync(customer => customer.Id == id && customer.TenantId == tenantId);
        if (customer is null)
        {
            return Results.NotFound();
        }

        customer.Name = request.Name.Trim();
        customer.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        customer.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        customer.BillingAddress = string.IsNullOrWhiteSpace(request.BillingAddress) ? null : request.BillingAddress.Trim();
        customer.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync();

        return Results.Ok(customer.Adapt<CustomerResponse>());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IResult> DeleteCustomer(Guid id)
    {
        var tenantId = Request.GetTenantId()!.Value;
        var customer = await db.Customers.SingleOrDefaultAsync(customer => customer.Id == id && customer.TenantId == tenantId);
        if (customer is null)
        {
            return Results.NotFound();
        }

        var hasInvoices = await db.Invoices.AnyAsync(invoice => invoice.CustomerId == id && invoice.TenantId == tenantId);
        if (hasInvoices)
        {
            return Results.Conflict(new { message = "Customer cannot be deleted while invoices reference it." });
        }

        db.Customers.Remove(customer);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}

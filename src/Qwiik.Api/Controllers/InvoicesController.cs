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
[Route("invoices")]
[ValidateTenant]
public class InvoicesController(AppDbContext db, ILogger<InvoicesController> logger) : ControllerBase
{
    private const int MaxPageSize = 100;

    [HttpGet]
    public async Task<IResult> GetInvoices([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var tenantId = Request.GetTenantId()!.Value;
        var query = db.Invoices
            .AsNoTracking()
            .Include(invoice => invoice.Customer)
            .Include(invoice => invoice.Items)
            .Where(invoice => invoice.TenantId == tenantId)
            .OrderByDescending(invoice => invoice.IssueDate)
            .ThenByDescending(invoice => invoice.CreatedAt);

        var totalCount = await query.CountAsync();
        var invoices = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Results.Ok(new PagedResponse<InvoiceResponse>
        {
            Items = invoices.Adapt<List<InvoiceResponse>>(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IResult> GetInvoice(Guid id)
    {
        var tenantId = Request.GetTenantId()!.Value;
        var invoice = await db.Invoices
            .AsNoTracking()
            .Include(invoice => invoice.Customer)
            .Include(invoice => invoice.Items)
            .SingleOrDefaultAsync(invoice => invoice.Id == id && invoice.TenantId == tenantId);

        return invoice is null
            ? Results.NotFound()
            : Results.Ok(invoice.Adapt<InvoiceResponse>());
    }

    [HttpPost]
    public async Task<IResult> CreateInvoice(CreateInvoiceRequest request)
    {
        var tenantId = Request.GetTenantId()!.Value;
        var validationResult = ValidateInvoiceDates(request.IssueDate, request.DueDate);
        if (validationResult is not null)
            return validationResult;

        validationResult = ValidateInvoiceItems(request.Items);
        if (validationResult is not null)
            return validationResult;

        var invoiceNumber = request.InvoiceNumber.Trim();

        var invoiceNumberExists = await db.Invoices.AnyAsync(invoice =>
            invoice.TenantId == tenantId && invoice.InvoiceNumber == invoiceNumber);
        if (invoiceNumberExists)
        {
            return Results.Conflict(new { message = "Invoice number already exists for this tenant." });
        }

        var customerExists = await db.Customers.AnyAsync(customer =>
            customer.Id == request.CustomerId && customer.TenantId == tenantId);
        if (!customerExists)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(CreateInvoiceRequest.CustomerId)] = ["Customer does not exist for this tenant."]
            });
        }

        var items = CreateInvoiceItems(tenantId, request.Items);
        var subtotal = items.Sum(item => item.LineTotal);
        var invoice = new Invoice
        {
            TenantId = tenantId,
            InvoiceNumber = invoiceNumber,
            CustomerId = request.CustomerId,
            IssueDate = request.IssueDate,
            DueDate = request.DueDate,
            Subtotal = subtotal,
            Tax = request.Tax,
            Total = subtotal + request.Tax,
            Status = request.Status,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            Items = items
        };

        db.Invoices.Add(invoice);
        await db.SaveChangesAsync();
        logger.LogInformation("Created invoice {InvoiceId} for tenant {TenantId}", invoice.Id, tenantId);

        await db.Entry(invoice).Reference(savedInvoice => savedInvoice.Customer).LoadAsync();
        await db.Entry(invoice).Collection(savedInvoice => savedInvoice.Items).LoadAsync();

        return Results.Created($"/invoices/{invoice.Id}", invoice.Adapt<InvoiceResponse>());
    }

    [HttpPut("{id:guid}")]
    public async Task<IResult> UpdateInvoice(Guid id, UpdateInvoiceRequest request)
    {
        var tenantId = Request.GetTenantId()!.Value;
        var validationResult = ValidateInvoiceDates(request.IssueDate, request.DueDate);
        if (validationResult is not null)
        {
            return validationResult;
        }

        validationResult = ValidateInvoiceItems(request.Items);
        if (validationResult is not null)
        {
            return validationResult;
        }

        var invoice = await db.Invoices
            .Include(invoice => invoice.Items)
            .SingleOrDefaultAsync(invoice => invoice.Id == id && invoice.TenantId == tenantId);
        if (invoice is null)
        {
            return Results.NotFound();
        }

        var invoiceNumber = request.InvoiceNumber.Trim();
        var invoiceNumberExists = await db.Invoices.AnyAsync(existingInvoice =>
            existingInvoice.Id != id &&
            existingInvoice.TenantId == tenantId &&
            existingInvoice.InvoiceNumber == invoiceNumber);
        if (invoiceNumberExists)
            return Results.Conflict(new { message = "Invoice number already exists for this tenant." });

        var customerExists = await db.Customers.AnyAsync(customer =>
            customer.Id == request.CustomerId && customer.TenantId == tenantId);
        if (!customerExists)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(UpdateInvoiceRequest.CustomerId)] = ["Customer does not exist for this tenant."]
            });
        }

        var items = CreateInvoiceItems(tenantId, request.Items);
        var subtotal = items.Sum(item => item.LineTotal);
        invoice.InvoiceNumber = invoiceNumber;
        invoice.CustomerId = request.CustomerId;
        invoice.IssueDate = request.IssueDate;
        invoice.DueDate = request.DueDate;
        invoice.Subtotal = subtotal;
        invoice.Tax = request.Tax;
        invoice.Total = subtotal + request.Tax;
        invoice.Status = request.Status;
        invoice.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        invoice.UpdatedAt = DateTimeOffset.UtcNow;
        db.InvoiceItems.RemoveRange(invoice.Items);
        invoice.Items = items;

        await db.SaveChangesAsync();
        logger.LogInformation("Updated invoice {InvoiceId} for tenant {TenantId}", invoice.Id, tenantId);

        await db.Entry(invoice).Reference(savedInvoice => savedInvoice.Customer).LoadAsync();
        await db.Entry(invoice).Collection(savedInvoice => savedInvoice.Items).LoadAsync();

        return Results.Ok(invoice.Adapt<InvoiceResponse>());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IResult> DeleteInvoice(Guid id)
    {
        var tenantId = Request.GetTenantId()!.Value;
        logger.LogInformation("Deleting invoice {InvoiceId} for tenant {TenantId}", id, tenantId);

        var invoice = await db.Invoices.SingleOrDefaultAsync(invoice => invoice.Id == id && invoice.TenantId == tenantId);
        if (invoice is null)
        {
            return Results.NotFound();
        }

        db.Invoices.Remove(invoice);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    private static IResult? ValidateInvoiceDates(DateOnly issueDate, DateOnly? dueDate)
    {
        if (dueDate < issueDate)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(CreateInvoiceRequest.DueDate)] = ["Due date must be on or after issue date."]
            });
        }

        return null;
    }

    private static IResult? ValidateInvoiceItems(IReadOnlyCollection<InvoiceItemRequest>? items)
    {
        if (items is null || items.Count == 0)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(CreateInvoiceRequest.Items)] = ["At least one invoice item is required."]
            });
        }

        if (items.Any(item => string.IsNullOrWhiteSpace(item.Description)))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(CreateInvoiceRequest.Items)] = ["Each invoice item requires a description."]
            });
        }

        return null;
    }

    private static List<InvoiceItem> CreateInvoiceItems(Guid tenantId, IReadOnlyList<InvoiceItemRequest> items)
    {
        return items.Select((item, index) => new InvoiceItem
        {
            TenantId = tenantId,
            Position = index + 1,
            Description = item.Description.Trim(),
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            LineTotal = item.Quantity * item.UnitPrice
        }).ToList();
    }
}

namespace Qwiik.Api.Extensions;

public static class RequestExtensions
{
    public const string TenantIdHeaderName = "X-Tenant-Id";

    public static Guid? GetTenantId(this HttpRequest request)
    {
        if (request.Headers.TryGetValue(TenantIdHeaderName, out var tenantIdHeader))
            return Guid.TryParse(tenantIdHeader.FirstOrDefault() ?? Guid.Empty.ToString(), out var tenantId) ? tenantId : null;

        return null;
    }
}

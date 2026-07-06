using Mapster;
using Qwiik.Api.Data.Models;
using Qwiik.Api.Requests;

namespace Qwiik.Api.Mapping;

public static class MapsterConfig
{
    public static void Register()
    {
        TypeAdapterConfig<Invoice, InvoiceResponse>
            .NewConfig()
            .Map(
                destination => destination.Items,
                source => source.Items
                    .OrderBy(item => item.Position)
                    .Adapt<List<InvoiceItemResponse>>());
    }
}

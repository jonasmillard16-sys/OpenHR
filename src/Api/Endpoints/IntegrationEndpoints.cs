using RegionHR.SharedKernel.Domain;

namespace RegionHR.Api.Endpoints;

public static class IntegrationEndpoints
{
    public static WebApplication MapIntegrationEndpoints(this WebApplication app)
    {
        var integ = app.MapGroup("/api/v1/integration").WithTags("Integrationer").RequireAuthorization("Systemadmin");

        integ.MapGet("/adapters", () =>
        {
            var adapters = new[]
            {
                new { System = "Skatteverket", Typ = "Extern", Riktning = "Ut", Frekvens = "Månadsvis", Format = "AGI XML v1.1" },
                new { System = "Nordea", Typ = "Extern", Riktning = "Ut", Frekvens = "Per lönekörning", Format = "ISO 20022 pain.001" },
                new { System = "Försäkringskassan", Typ = "Extern", Riktning = "Båda", Frekvens = "Vid behov", Format = "E-tjänst/API" },
                new { System = "Kronofogden", Typ = "Extern", Riktning = "Båda", Frekvens = "Vid behov", Format = "E-tjänst" },
                new { System = "Skandia", Typ = "Extern", Riktning = "Ut", Frekvens = "Månadsvis", Format = "Pipe-separated" },
                new { System = "SKR", Typ = "Extern", Riktning = "Ut", Frekvens = "Årlig (november)", Format = "Fast bredd" },
                new { System = "SCB (KLR)", Typ = "Extern", Riktning = "Ut", Frekvens = "Månadsvis (25:e)", Format = "Fast bredd" },
                new { System = "Raindance", Typ = "Intern", Riktning = "Ut", Frekvens = "Per lönekörning", Format = "Semikolon-separerad" },
                new { System = "KOLL/HOSP", Typ = "Intern", Riktning = "In", Frekvens = "Vid behov", Format = "REST API" },
                new { System = "Epassi", Typ = "Intern", Riktning = "Ut", Frekvens = "Månadsvis", Format = "CSV via SFTP" },
                new { System = "Troman", Typ = "Intern", Riktning = "Båda", Frekvens = "Vid behov", Format = "REST API" },
                new { System = "PowerBI", Typ = "Intern", Riktning = "Ut", Frekvens = "Daglig", Format = "JSON" },
                new { System = "Grade (LMS)", Typ = "Intern", Riktning = "Båda", Frekvens = "Daglig", Format = "SCIM/REST" },
                new { System = "Min kompetens", Typ = "Intern", Riktning = "Båda", Frekvens = "Vid behov", Format = "REST API" },
                new { System = "Diver", Typ = "Intern", Riktning = "Ut", Frekvens = "Veckovis", Format = "CSV" },
                new { System = "Microweb (arkiv)", Typ = "Intern", Riktning = "Ut", Frekvens = "Vid behov", Format = "API/fil" },
            };
            return Results.Ok(new { AntalIntegrationer = adapters.Length, Adapters = adapters });
        }).WithName("ListIntegrationAdapters");

        return app;
    }
}

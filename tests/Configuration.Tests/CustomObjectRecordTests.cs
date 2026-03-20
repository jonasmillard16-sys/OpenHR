using RegionHR.Configuration.Domain;
using Xunit;

namespace RegionHR.Configuration.Tests;

public class CustomObjectRecordTests
{
    [Fact]
    public void Skapa_SetsAllProperties()
    {
        var objectId = Guid.NewGuid();
        var data = """{"Typ":"Dator","Serienummer":"SN-001"}""";

        var record = CustomObjectRecord.Skapa(objectId, data, "Anna Svensson");

        Assert.NotEqual(Guid.Empty, record.Id);
        Assert.Equal(objectId, record.CustomObjectId);
        Assert.Equal(data, record.Data);
        Assert.Equal("Anna Svensson", record.SkapadAv);
        Assert.Null(record.UppdateradVid);
    }

    [Fact]
    public void Skapa_DefaultsSkapadAvToSystem()
    {
        var record = CustomObjectRecord.Skapa(Guid.NewGuid(), "{}");

        Assert.Equal("System", record.SkapadAv);
    }

    [Fact]
    public void UppdateraData_ChangesDataAndSetsUppdateradVid()
    {
        var record = CustomObjectRecord.Skapa(Guid.NewGuid(), """{"old":"data"}""");
        Assert.Null(record.UppdateradVid);

        var newData = """{"new":"data"}""";
        record.UppdateraData(newData);

        Assert.Equal(newData, record.Data);
        Assert.NotNull(record.UppdateradVid);
    }

    [Fact]
    public void Skapa_SetsSkapadVidToApproximatelyUtcNow()
    {
        var before = DateTime.UtcNow;
        var record = CustomObjectRecord.Skapa(Guid.NewGuid(), "{}");
        var after = DateTime.UtcNow;

        Assert.InRange(record.SkapadVid, before, after);
    }

    [Fact]
    public void ValidateDataAgainstSchema_RequiredFieldMissing_ReturnsError()
    {
        var schema = """[{"name":"Serienummer","type":"Text","required":true}]""";
        var data = """{}""";

        var error = RegionHR.Api.Endpoints.CustomObjectEndpoints.ValidateDataAgainstSchema(data, schema);

        Assert.NotNull(error);
        Assert.Contains("Serienummer", error);
    }

    [Fact]
    public void ValidateDataAgainstSchema_ValidData_ReturnsNull()
    {
        var schema = """[{"name":"Serienummer","type":"Text","required":true},{"name":"Tilldelad","type":"Text","required":false}]""";
        var data = """{"Serienummer":"SN-001"}""";

        var error = RegionHR.Api.Endpoints.CustomObjectEndpoints.ValidateDataAgainstSchema(data, schema);

        Assert.Null(error);
    }

    [Fact]
    public void ValidateDataAgainstSchema_InvalidNumber_ReturnsError()
    {
        var schema = """[{"name":"Antal","type":"Number","required":true}]""";
        var data = """{"Antal":"abc"}""";

        var error = RegionHR.Api.Endpoints.CustomObjectEndpoints.ValidateDataAgainstSchema(data, schema);

        Assert.NotNull(error);
        Assert.Contains("nummer", error);
    }

    [Fact]
    public void ValidateDataAgainstSchema_InvalidDropdownOption_ReturnsError()
    {
        var schema = """[{"name":"Typ","type":"Dropdown","required":true,"options":["Dator","Telefon"]}]""";
        var data = """{"Typ":"Cykel"}""";

        var error = RegionHR.Api.Endpoints.CustomObjectEndpoints.ValidateDataAgainstSchema(data, schema);

        Assert.NotNull(error);
        Assert.Contains("ogiltigt varde", error);
    }

    [Fact]
    public void ValidateDataAgainstSchema_ValidDropdownOption_ReturnsNull()
    {
        var schema = """[{"name":"Typ","type":"Dropdown","required":true,"options":["Dator","Telefon"]}]""";
        var data = """{"Typ":"Dator"}""";

        var error = RegionHR.Api.Endpoints.CustomObjectEndpoints.ValidateDataAgainstSchema(data, schema);

        Assert.Null(error);
    }

    [Fact]
    public void ValidateDataAgainstSchema_InvalidEmail_ReturnsError()
    {
        var schema = """[{"name":"Epost","type":"Email","required":true}]""";
        var data = """{"Epost":"notanemail"}""";

        var error = RegionHR.Api.Endpoints.CustomObjectEndpoints.ValidateDataAgainstSchema(data, schema);

        Assert.NotNull(error);
        Assert.Contains("e-postadress", error);
    }

    [Fact]
    public void ValidateDataAgainstSchema_InvalidJson_ReturnsError()
    {
        var schema = """[{"name":"Falt","type":"Text","required":true}]""";
        var data = "not json";

        var error = RegionHR.Api.Endpoints.CustomObjectEndpoints.ValidateDataAgainstSchema(data, schema);

        Assert.NotNull(error);
        Assert.Contains("JSON", error);
    }
}

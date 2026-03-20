using RegionHR.Configuration.Domain;
using Xunit;

namespace RegionHR.Configuration.Tests;

public class CustomObjectTests
{
    [Fact]
    public void Skapa_SetsAllProperties()
    {
        var schema = """[{"name":"Typ","type":"Dropdown","required":true,"options":["Dator","Telefon"]}]""";
        var obj = CustomObject.Skapa("Utrustning", "Utrustningar", "Register", schema, null, "Devices");

        Assert.NotEqual(Guid.Empty, obj.Id);
        Assert.Equal("Utrustning", obj.Namn);
        Assert.Equal("Utrustningar", obj.PluralNamn);
        Assert.Equal("Register", obj.Beskrivning);
        Assert.Equal(schema, obj.FaltSchema);
        Assert.Equal("[]", obj.Relationer);
        Assert.Equal("Devices", obj.Ikon);
    }

    [Fact]
    public void Skapa_DefaultsRealtionerToEmptyArray()
    {
        var obj = CustomObject.Skapa("Test", "Tester", "Desc", "[]");
        Assert.Equal("[]", obj.Relationer);
        Assert.Equal("Extension", obj.Ikon);
    }

    [Fact]
    public void UppdateraSchema_ChangesSchema()
    {
        var obj = CustomObject.Skapa("Test", "Tester", "Desc", "[]");
        var newSchema = """[{"name":"Falt1","type":"Text","required":true}]""";

        obj.UppdateraSchema(newSchema);

        Assert.Equal(newSchema, obj.FaltSchema);
    }

    [Fact]
    public void UppdateraBeskrivning_ChangesBeskrivning()
    {
        var obj = CustomObject.Skapa("Test", "Tester", "Desc", "[]");

        obj.UppdateraBeskrivning("Ny beskrivning");

        Assert.Equal("Ny beskrivning", obj.Beskrivning);
    }

    [Theory]
    [InlineData("Text")]
    [InlineData("Number")]
    [InlineData("Date")]
    [InlineData("Dropdown")]
    [InlineData("MultiSelect")]
    [InlineData("YesNo")]
    [InlineData("Email")]
    [InlineData("Phone")]
    [InlineData("URL")]
    public void CustomObjectFieldType_IsValid_ReturnsTrue(string type)
    {
        Assert.True(CustomObjectFieldType.IsValid(type));
    }

    [Fact]
    public void CustomObjectFieldType_IsValid_ReturnsFalseForInvalid()
    {
        Assert.False(CustomObjectFieldType.IsValid("Invalid"));
    }

    [Fact]
    public void CustomObjectFieldType_All_Contains9Types()
    {
        Assert.Equal(9, CustomObjectFieldType.All.Length);
    }

    [Fact]
    public void Skapa_SetsSkapadVidToApproximatelyUtcNow()
    {
        var before = DateTime.UtcNow;
        var obj = CustomObject.Skapa("Test", "Tester", "Desc", "[]");
        var after = DateTime.UtcNow;

        Assert.InRange(obj.SkapadVid, before, after);
    }
}

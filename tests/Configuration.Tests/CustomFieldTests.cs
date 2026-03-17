using RegionHR.Configuration.Domain;
using Xunit;

namespace RegionHR.Configuration.Tests;

public class CustomFieldTests
{
    [Fact]
    public void Skapa_SetsAllProperties()
    {
        var field = CustomField.Skapa(
            "blodgrupp", "Blodgrupp", CustomFieldType.Valval,
            CustomFieldTarget.Anstalld, true,
            """["A+","A-","B+","B-","O+","O-","AB+","AB-"]""", 5);

        Assert.NotEqual(Guid.Empty, field.Id);
        Assert.Equal("blodgrupp", field.FieldName);
        Assert.Equal("Blodgrupp", field.DisplayName);
        Assert.Equal(CustomFieldType.Valval, field.FieldType);
        Assert.Equal(CustomFieldTarget.Anstalld, field.Target);
        Assert.True(field.ArObligatorisk);
        Assert.Contains("A+", field.Alternativ!);
        Assert.Equal(5, field.Ordning);
        Assert.True(field.ArAktiv);
    }

    [Fact]
    public void Skapa_DefaultsObligatoriskToFalse()
    {
        var field = CustomField.Skapa("test", "Test", CustomFieldType.Text, CustomFieldTarget.Anstalld);

        Assert.False(field.ArObligatorisk);
    }

    [Fact]
    public void Inaktivera_SetsArAktivToFalse()
    {
        var field = CustomField.Skapa("test", "Test", CustomFieldType.Text, CustomFieldTarget.Anstalld);
        Assert.True(field.ArAktiv);

        field.Inaktivera();

        Assert.False(field.ArAktiv);
    }

    [Theory]
    [InlineData(CustomFieldType.Text)]
    [InlineData(CustomFieldType.Nummer)]
    [InlineData(CustomFieldType.Datum)]
    [InlineData(CustomFieldType.Valval)]
    [InlineData(CustomFieldType.Flerval)]
    [InlineData(CustomFieldType.JaNej)]
    public void CustomFieldType_HasExpectedValues(CustomFieldType type)
    {
        var field = CustomField.Skapa("f", "F", type, CustomFieldTarget.Anstalld);
        Assert.Equal(type, field.FieldType);
    }

    [Theory]
    [InlineData(CustomFieldTarget.Anstalld)]
    [InlineData(CustomFieldTarget.Anstallning)]
    [InlineData(CustomFieldTarget.Organisation)]
    [InlineData(CustomFieldTarget.Arende)]
    [InlineData(CustomFieldTarget.Vakans)]
    public void CustomFieldTarget_HasExpectedValues(CustomFieldTarget target)
    {
        var field = CustomField.Skapa("f", "F", CustomFieldType.Text, target);
        Assert.Equal(target, field.Target);
    }

    [Fact]
    public void CustomFieldType_ContainsSixValues()
    {
        Assert.Equal(6, Enum.GetValues<CustomFieldType>().Length);
    }

    [Fact]
    public void CustomFieldTarget_ContainsFiveValues()
    {
        Assert.Equal(5, Enum.GetValues<CustomFieldTarget>().Length);
    }
}

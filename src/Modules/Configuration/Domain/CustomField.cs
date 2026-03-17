namespace RegionHR.Configuration.Domain;

public enum CustomFieldType { Text, Nummer, Datum, Valval, Flerval, JaNej }
public enum CustomFieldTarget { Anstalld, Anstallning, Organisation, Arende, Vakans }

public class CustomField
{
    public Guid Id { get; private set; }
    public string FieldName { get; private set; } = "";
    public string DisplayName { get; private set; } = "";
    public CustomFieldType FieldType { get; private set; }
    public CustomFieldTarget Target { get; private set; }
    public bool ArObligatorisk { get; private set; }
    public string? Alternativ { get; private set; } // JSON array for Valval/Flerval
    public string? Standardvarde { get; private set; }
    public int Ordning { get; private set; }
    public bool ArAktiv { get; private set; }

    private CustomField() { }

    public static CustomField Skapa(string fieldName, string displayName, CustomFieldType type, CustomFieldTarget target, bool obligatorisk = false, string? alternativ = null, int ordning = 0)
    {
        return new CustomField
        {
            Id = Guid.NewGuid(), FieldName = fieldName, DisplayName = displayName,
            FieldType = type, Target = target, ArObligatorisk = obligatorisk,
            Alternativ = alternativ, Ordning = ordning, ArAktiv = true
        };
    }

    public void Inaktivera() { ArAktiv = false; }
}

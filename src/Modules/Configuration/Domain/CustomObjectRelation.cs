namespace RegionHR.Configuration.Domain;

public class CustomObjectRelation
{
    public Guid Id { get; private set; }
    public Guid CustomObjectId { get; private set; }
    public string KallEntityTyp { get; private set; } = ""; // Employee, Employment, OrganizationUnit, Case, Vacancy
    public string RelationsTyp { get; private set; } = ""; // OneToMany, ManyToMany

    private CustomObjectRelation() { }

    public static CustomObjectRelation Skapa(Guid customObjectId, string kallEntityTyp, string relationsTyp)
    {
        return new CustomObjectRelation
        {
            Id = Guid.NewGuid(),
            CustomObjectId = customObjectId,
            KallEntityTyp = kallEntityTyp,
            RelationsTyp = relationsTyp
        };
    }
}

public static class KallEntityTyper
{
    public const string Employee = "Employee";
    public const string Employment = "Employment";
    public const string OrganizationUnit = "OrganizationUnit";
    public const string Case = "Case";
    public const string Vacancy = "Vacancy";

    public static readonly string[] All = [Employee, Employment, OrganizationUnit, Case, Vacancy];
    public static bool IsValid(string typ) => All.Contains(typ);
}

public static class RelationsTyper
{
    public const string OneToMany = "OneToMany";
    public const string ManyToMany = "ManyToMany";

    public static readonly string[] All = [OneToMany, ManyToMany];
    public static bool IsValid(string typ) => All.Contains(typ);
}

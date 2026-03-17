namespace RegionHR.Infrastructure.Authorization;

public class UnitScopeService
{
    // In production, this reads from JWT claims. For now, return all access.
    public bool HasAccessToUnit(Guid unitId, string userId, string role)
    {
        // Systemadmin and HR have access to all units
        if (role is "Systemadmin" or "HR" or "Loneadmin") return true;
        // Chef and Anstalld access checked per unit (placeholder - returns true for dev)
        return true;
    }

    public bool HasAccessToEmployee(Guid employeeId, string userId, string role)
    {
        if (role is "Systemadmin" or "HR" or "Loneadmin") return true;
        return true; // placeholder
    }
}

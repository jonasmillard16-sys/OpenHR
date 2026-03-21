namespace RegionHR.VMS.Domain;

/// <summary>Leverantörsstatus</summary>
public enum VendorStatus
{
    Active,
    Inactive,
    Blocked
}

/// <summary>Status för bemanningsbeställning</summary>
public enum StaffingRequestStatus
{
    Draft,
    Submitted,
    Approved,
    Filled,
    Closed
}

/// <summary>Status för tidrapport</summary>
public enum TimeReportStatus
{
    Draft,
    Submitted,
    Attested
}

/// <summary>Status för leverantörsfaktura</summary>
public enum VendorInvoiceStatus
{
    Received,
    Matched,
    Approved,
    Paid
}

/// <summary>F-skattstatus</summary>
public enum FSkattStatus
{
    Active,
    Inactive,
    Unknown
}

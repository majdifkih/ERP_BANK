namespace ERP.Banking.Domain.Constants;

/// <summary>
/// Defines the canonical role names used throughout the application.
/// </summary>
public static class Roles
{
    public const string SuperAdmin = "SUPER_ADMIN";
    public const string BusinessAdmin = "BUSINESS_ADMIN";
    public const string BranchDirector = "BRANCH_DIRECTOR";
    public const string ServiceManager = "SERVICE_MANAGER";
    public const string BankingAgent = "BANKING_AGENT";
    public const string Auditor = "AUDITOR";
    public const string ReadOnly = "READ_ONLY";
}
namespace ERP.Banking.Domain.Constants;

/// <summary>
/// Defines all permission codes used for role-based access control (RBAC).
/// These values are stored in the database and matched at authorization time.
/// </summary>
public static class Permissions
{
    // ── User Management ────────────────────────────────────────────
    public const string UserCreate = "USER_CREATE";
    public const string UserUpdate = "USER_UPDATE";
    public const string UserDelete = "USER_DELETE";
    public const string UserRead = "USER_READ";

    // ── Role Management ────────────────────────────────────────────
    public const string RoleManage = "ROLE_MANAGE";

    // ── Client Management ──────────────────────────────────────────
    public const string ClientCreate = "CLIENT_CREATE";
    public const string ClientUpdate = "CLIENT_UPDATE";
    public const string ClientRead = "CLIENT_READ";

    // ── Account Management ─────────────────────────────────────────
    public const string AccountCreate = "ACCOUNT_CREATE";
    public const string AccountRead = "ACCOUNT_READ";

    // ── Credit Management ──────────────────────────────────────────
    public const string CreditApprove = "CREDIT_APPROVE";
}
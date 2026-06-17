namespace ERP.Bancaire.Domain.Constants;

public static class Permissions
{
    public const string UserCreate = "USER_CREATE";
    public const string UserUpdate = "USER_UPDATE";
    public const string UserDelete = "USER_DELETE";
    public const string UserRead = "USER_READ";

    public const string RoleManage = "ROLE_MANAGE";

    public const string ClientCreate = "CLIENT_CREATE";
    public const string ClientUpdate = "CLIENT_UPDATE";
    public const string ClientRead = "CLIENT_READ";

    public const string AccountCreate = "ACCOUNT_CREATE";
    public const string AccountRead = "ACCOUNT_READ";

    public const string CreditApprove = "CREDIT_APPROVE";
}
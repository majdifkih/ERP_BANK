using ERP.Banking.Domain.Constants;

namespace ERP.Banking.API.Extensions;

/// <summary>
/// Registers all permission-based authorization policies.
/// Each policy maps to a fine-grained permission claim embedded in the JWT.
/// </summary>
public static class AuthorizationExtensions
{
    public static IServiceCollection AddPermissionAuthorization(
        this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // ── User Management ────────────────────────────────────
            options.AddPolicy(Permissions.UserCreate,
                p => p.RequireClaim("permission", Permissions.UserCreate));
            options.AddPolicy(Permissions.UserUpdate,
                p => p.RequireClaim("permission", Permissions.UserUpdate));
            options.AddPolicy(Permissions.UserDelete,
                p => p.RequireClaim("permission", Permissions.UserDelete));
            options.AddPolicy(Permissions.UserRead,
                p => p.RequireClaim("permission", Permissions.UserRead));

            // ── Role Management ────────────────────────────────────
            options.AddPolicy(Permissions.RoleManage,
                p => p.RequireClaim("permission", Permissions.RoleManage));

            // ── Client Management ──────────────────────────────────
            options.AddPolicy(Permissions.ClientCreate,
                p => p.RequireClaim("permission", Permissions.ClientCreate));
            options.AddPolicy(Permissions.ClientUpdate,
                p => p.RequireClaim("permission", Permissions.ClientUpdate));
            options.AddPolicy(Permissions.ClientRead,
                p => p.RequireClaim("permission", Permissions.ClientRead));

            // ── Account Management ─────────────────────────────────
            options.AddPolicy(Permissions.AccountCreate,
                p => p.RequireClaim("permission", Permissions.AccountCreate));
            options.AddPolicy(Permissions.AccountRead,
                p => p.RequireClaim("permission", Permissions.AccountRead));

            // ── Credit Management ──────────────────────────────────
            options.AddPolicy(Permissions.CreditApprove,
                p => p.RequireClaim("permission", Permissions.CreditApprove));
        });

        return services;
    }
}
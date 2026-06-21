using Microsoft.Extensions.DependencyInjection;

namespace ERP.Banking.Application;

/// <summary>
/// Registers all Application-layer services into the DI container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        // Application layer currently holds interfaces and DTOs only.
        // Use Cases / Command Handlers (e.g. MediatR) would be registered here
        // as the application grows.
        return services;
    }
}
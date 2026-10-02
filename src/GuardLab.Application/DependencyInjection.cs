using GuardLab.Application.Health;
using GuardLab.Application.Labs;
using GuardLab.Application.Labs.IdorKeyAccess;
using GuardLab.Application.AdminAuth;
using GuardLab.Core.AdminAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace GuardLab.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddGuardLabApplication(this IServiceCollection services)
    {
        services.AddScoped<HealthService>();
        services.AddScoped<LabCatalogService>();
        services.AddScoped<LabAdminService>();
        services.AddScoped<LabRunService>();
        services.AddScoped<VulnerableIdorService>();
        services.AddScoped<SafeIdorService>();
        services.AddScoped<AdminAuthService>();
        services.AddScoped<IPasswordHasher<AdminUser>, PasswordHasher<AdminUser>>();
        return services;
    }
}

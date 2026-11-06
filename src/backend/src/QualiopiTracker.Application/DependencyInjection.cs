namespace QualiopiTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<ProviderProfileService>();
        services.AddScoped<FrameworkService>();

        return services;
    }
}

namespace InventoryLab3.Infrastructure;

public static class CorsSetup
{
    public const string PolicyName = "Frontend";

    public static IServiceCollection AddFrontendCors(this IServiceCollection services, IConfiguration config)
    {
        var origins = config.GetSection("Cors:Origins").Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                policy.WithOrigins(origins)
                      .WithHeaders("Authorization", "Content-Type")
                      .WithMethods("GET", "POST", "PUT", "DELETE");
            });
        });

        return services;
    }
}

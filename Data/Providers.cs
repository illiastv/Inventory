using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryLab3.Data;

public class SqliteAppDbContext(DbContextOptions<SqliteAppDbContext> options) : AppDbContext(options);
public class PostgresAppDbContext(DbContextOptions<PostgresAppDbContext> options) : AppDbContext(options);

public static class DatabaseExtensions
{
    public static IServiceCollection AddInventoryDatabase(this IServiceCollection services, IConfiguration config)
    {
        var provider = config["Database:Provider"] ?? "Sqlite";

        switch (provider.ToLowerInvariant())
        {
            case "sqlite":
                services.AddDbContext<SqliteAppDbContext>(o =>
                    o.UseSqlite(config.GetConnectionString("Sqlite")));
                services.AddScoped<AppDbContext>(sp => sp.GetRequiredService<SqliteAppDbContext>());
                break;

            case "postgres":
                services.AddDbContext<PostgresAppDbContext>(o =>
                    o.UseNpgsql(config.GetConnectionString("Postgres")));
                services.AddScoped<AppDbContext>(sp => sp.GetRequiredService<PostgresAppDbContext>());
                break;

            default:
                throw new InvalidOperationException($"Невідомий провайдер БД: '{provider}'. Допустимо: Sqlite, Postgres.");
        }

        return services;
    }
}

public class SqliteDesignFactory : IDesignTimeDbContextFactory<SqliteAppDbContext>
{
    public SqliteAppDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<SqliteAppDbContext>().UseSqlite("Data Source=design.db").Options);
}

public class PostgresDesignFactory : IDesignTimeDbContextFactory<PostgresAppDbContext>
{
    public PostgresAppDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<PostgresAppDbContext>()
            .UseNpgsql("Host=localhost;Database=inventory;Username=postgres;Password=postgres").Options);
}

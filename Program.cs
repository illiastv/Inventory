using InventoryLab3.Data;
using InventoryLab3.Infrastructure;
using InventoryLab3.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInventoryDatabase(builder.Configuration);
builder.Services.AddJwtAuth(builder.Configuration);
builder.Services.AddFrontendCors(builder.Configuration);
builder.Services.AddSwaggerWithBearer();

builder.Services.AddSingleton<TokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<WarehouseService>();
builder.Services.AddScoped<StockService>();
builder.Services.AddScoped<ReportService>();

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseExceptionHandler();
if (!app.Environment.IsDevelopment())
    app.UseHsts();
app.UseHttpsRedirection();
app.UseStatusCodePages();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(CorsSetup.PolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!db.Database.GetMigrations().Any())
    {
        throw new InvalidOperationException(
            "Немає міграцій. Створи їх: " +
            "dotnet ef migrations add Initial --context SqliteAppDbContext --output-dir Migrations/Sqlite");
    }

    await db.Database.MigrateAsync();

    var adminEmail = app.Configuration["Seed:AdminEmail"] ?? "admin@example.com";
    var adminPassword = app.Configuration["Seed:AdminPassword"];

    if (string.IsNullOrWhiteSpace(adminPassword))
    {
        app.Logger.LogWarning("Seed:AdminPassword не задано, адміністратора не створено");
    }
    else
    {
        var auth = scope.ServiceProvider.GetRequiredService<AuthService>();
        await auth.SeedAdmin(adminEmail, adminPassword);
    }
}

app.Run();

using AzureSqlTokenAuth.Auth;
using AzureSqlTokenAuth.Configuration;
using AzureSqlTokenAuth.Data;
using AzureSqlTokenAuth.Services;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add configurations
builder.Services.Configure<AzureAdOptions>(
    builder.Configuration.GetSection(AzureAdOptions.SectionName));
builder.Services.Configure<SqlDatabaseOptions>(
    builder.Configuration.GetSection(SqlDatabaseOptions.SectionName));

// Add services
builder.Services.AddSingleton<ITokenManager, TokenManager>();
builder.Services.AddSingleton<IConnectionManager, ConnectionManager>();
builder.Services.AddSingleton<IDatabaseInitializer, DatabaseInitializer>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
var app = builder.Build();

// Initialize the database
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
        await dbInitializer.InitializeAsync();

        // Seed sample data in development
        if (app.Environment.IsDevelopment())
        {
            app.Logger.LogInformation("Development environment detected. Seeding sample data...");
            await dbInitializer.SeedSampleDataAsync();
        }
    }
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "An error occurred while initializing the database");
    throw;
}


// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

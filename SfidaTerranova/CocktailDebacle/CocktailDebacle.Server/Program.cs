using CocktailDebacle.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CocktailDebacle.Server.Service;


/*
local => Server=MSI; Database=CocktailDebacle; Trusted_Connection=True; TrustServerCertificate=True; MultipleActiveResultSets=True;
Docket => Server=sqlserver;Database=CocktailDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;
*/
var builder = WebApplication.CreateBuilder(args);
var MyallowSpecificOrigins = "_myAllowSpecificOrigins";

// Configurazione CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyallowSpecificOrigins,
        builder =>
        {
            builder.WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

// Aggiungi servizi al container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurazione del DbContext con retry policy per gestire i tentativi di connessione
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });
});

var token = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(token))
{
    throw new ArgumentNullException("JWT key is not configured.");
}

// Configurazione JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(token)),
            ValidateIssuer = false,
            ValidateAudience = false, 
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // Riduci il tempo di tolleranza per la scadenza del token
        };
    });

builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings")
);

builder.Services.AddSingleton<CloudinaryService>();
builder.Services.AddScoped<IAuthService, AuthService>(); // aggiungi cocketail service
builder.Services.AddHttpClient<CocktailImportService>(); // aggiungi cocketail service

var openAiApiKey = builder.Configuration["OpenAI:ApiKey"];

if (string.IsNullOrEmpty(openAiApiKey))
{
    throw new ArgumentNullException("OpenAI API key is not configured.");
}

builder.Services.AddSingleton(new OpenAIService(openAiApiKey));
builder.Services.AddSingleton<RecommenderEngine>();

var app = builder.Build();

// Applica le migrazioni e crea il database se non esiste
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        
        // Attendi che SQL Server sia pronto (solo in ambiente Docker)
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            await WaitForSqlServer(dbContext);
        }
        
        dbContext.Database.Migrate();
        Console.WriteLine("Database migrated successfully.");

        // Importa i cocktail
        var cocktailImportService = services.GetRequiredService<CocktailImportService>();
        await cocktailImportService.ImportCocktailsAsync();
        Console.WriteLine("Cocktails imported successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while migrating the database: {ex.Message}");
    }
}

app.UseDefaultFiles();
app.UseStaticFiles();

// Configura pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Access-Control-Allow-Origin", "http://localhost:4200");
    context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
    context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization");
    context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
    await next();
});

app.UseCors(MyallowSpecificOrigins);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("/index.html");

app.Run();

// Helper function per attendere che SQL Server sia pronto
async Task WaitForSqlServer(AppDbContext dbContext, int maxAttempts = 10, int delaySeconds = 5)
{
    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            Console.WriteLine($"Attempting to connect to SQL Server (attempt {attempt}/{maxAttempts})...");
            if (await dbContext.Database.CanConnectAsync())
            {
                Console.WriteLine("Successfully connected to SQL Server.");
                return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Attempt {attempt} failed: {ex.Message}");
            if (attempt == maxAttempts)
                throw;
        }
        
        await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
    }
}
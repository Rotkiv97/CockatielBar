using CocktailDebacle.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CocktailDebacle.Server.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;


/*
local => Server=localhost\\SQLEXPRESS; Database=CocktailDebacle; Trusted_Connection=True; TrustServerCertificate=True; MultipleActiveResultSets=True;
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

// servizi al container
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
            ClockSkew = TimeSpan.Zero
        };
        // getione dei tocken scaduti
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                var rawToken = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                var user = await dbContext.DbUser.FirstOrDefaultAsync(u => u.Token == rawToken);

                if (user == null || user.TokenExpiration == null || user.TokenExpiration <= DateTime.UtcNow)
                {
                    context.Fail("Token not valid anymore.");
                }
            }
        };
    });

    builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings")
);

builder.Services.AddSingleton<CloudinaryService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<ICleanTokenHostedService, CleanTokenHostedService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<ICleanTokenHostedService>());
builder.Services.AddHttpClient<CocktailImportService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<TranslationService>();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var app = builder.Build();

// Applica le migrazioni e crea il database se non esiste
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            await WaitForSqlServer(dbContext);
        }
        
        dbContext.Database.Migrate();

        var cocktailImportService = services.GetRequiredService<CocktailImportService>();
        await cocktailImportService.ImportCocktailsAsync();
        Console.WriteLine("Cocktails imported successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
    }
}

app.Use(async (context, next) => {
    var endpoint = context.GetEndpoint();
    if(endpoint?.Metadata.GetMetadata<IAuthorizeData>() != null)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        if (!string.IsNullOrEmpty(token))
        {
            var Dbcontext = context.RequestServices.GetRequiredService<AppDbContext>();
            var user = await Dbcontext.DbUser.FirstOrDefaultAsync(u => u.Token == token);
            if (user == null || user.TokenExpiration == null || user.TokenExpiration <= DateTime.UtcNow)
            {
                context.Response.StatusCode = 401;
                return;
            }
        }
    }
    await next();
});

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
            if (await dbContext.Database.CanConnectAsync())
            {
                Console.WriteLine("Successfully connected to SQL Server. 💡");
                return;
            }
        }
        catch 
        {
            if (attempt == maxAttempts)
                throw;
        }
        await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
    }
}
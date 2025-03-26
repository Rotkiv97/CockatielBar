using CocktailDebacle.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CocktailDebacle.Server.Service;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Configura il CORS
var MyallowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyallowSpecificOrigins,
        builder =>
        {
            builder.WithOrigins("http://localhost:4200")
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
        });
});

// Configura il Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<CocktailService>();
builder.Services.AddScoped<CocktailService>();

// Configura l'autenticazione con JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });

// Configura l'Autorizzazione
builder.Services.AddAuthorization();

// Registra il Servizio di Autenticazione
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SwaggerAC-TUACocktailDebacleAPI",
        Version = "v1"
    });
});

var app = builder.Build();

// using (var scope = app.Services.CreateScope())
// {
//     var services = scope.ServiceProvider;
//     var cocktailService = services.GetRequiredService<CocktailService>();
//     await cocktailService.ImportCocktailsAsync();
// }

//ATTIVA SWAGGER ANCHE IN PRODUZIONE SE NECESSARIO
if (app.Environment.IsDevelopment() || app.Environment.IsProduction()) 
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cocktail API V1");
        c.RoutePrefix = "swagger"; // Imposta il percorso di Swagger
    });
}

// Middleware
app.UseCors(MyallowSpecificOrigins);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

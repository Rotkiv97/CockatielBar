using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using CocktailDebacle.Server.Models;
using System.Threading.Tasks;

namespace CocktailDebacle.Server.Service
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<string> AuthenticateUser(string UserName, string password)
        {
            var user = await _context.DbUser.SingleOrDefaultAsync(u => u.UserName == UserName);
            
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null; // Utente non trovato o password errata
            }

            // Recupera la chiave JWT dalla configurazione
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new Exception("Errore: la chiave JWT è mancante nella configurazione! Assicurati che sia presente in appsettings.json.");
            }

            var key = Encoding.ASCII.GetBytes(jwtKey);
            
            // Generazione del token JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("AcceptCookies", user.AcceptCookies?.ToString() ?? string.Empty)
                }),
                Expires = DateTime.UtcNow.AddHours(2), // Durata del token
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}

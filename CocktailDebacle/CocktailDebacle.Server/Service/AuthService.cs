using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using CocktailDebacle.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using CocktailDebacle.Server.Service;


namespace CocktailDebacle.Server.Models
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

        public async Task<string> AuthenticateUser(string email, string password)
        {
            var user = await _context.DbUser.SingleOrDefaultAsync(u => u.Email == email);
            
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null; // Utente non trovato o password errata
            }

            // Generazione del token JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            // var secretKey = _configuration["Jwt:Key"];
            // if (string.IsNullOrEmpty(secretKey))
            // {
            //     throw new Exception("Errore: la chiave JWT è mancante nella configurazione! Assicurati che sia presente in appsettings.json.");
            // }

            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new Exception("Errore: la chiave JWT è mancante nella configurazione! Assicurati che sia presente in appsettings.json.");
            }
            var key = Encoding.ASCII.GetBytes(jwtKey);
            {
                throw new Exception("Errore: la chiave JWT è mancante nella configurazione! Assicurati che sia presente in appsettings.json.");
            }
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString() ?? string.Empty),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
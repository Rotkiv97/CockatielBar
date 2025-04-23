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

        private readonly ICleanTokenHostedService _cleanTokenHostedService;

        public AuthService(AppDbContext context, IConfiguration configuration, ICleanTokenHostedService cleanTokenHostedService)
        {
            _cleanTokenHostedService = cleanTokenHostedService;
            _context = context;
            _configuration = configuration;
        }

        public async Task<string> AuthenticateUser(string userName, string password, User user)
        {

            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("Username non valido", nameof(userName));

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            // Configurazione JWT
            var jwtKey = _configuration["Jwt:Key"] ?? throw new ApplicationException("Chiave JWT non configurata. Configurala in appsettings.json");

            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            // Calcolo scadenza
            var expiration = DateTime.UtcNow.AddHours(1);//DateTime.UtcNow.AddMinutes(1);
            
            // Creazione claims
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Aggiungi altri claim se presenti
            if (!string.IsNullOrEmpty(user.Email))
                claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));

            // Creazione token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiration,
                SigningCredentials = credentials,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(securityToken);

            // Aggiornamento utente e tracking scadenza
            user.Token = tokenString;
            user.TokenExpiration = expiration;
            
            // Registra per invalidazione automatica
            await _cleanTokenHostedService.TrackToken(user, expiration);

            return tokenString;
        }
    }
}
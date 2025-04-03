﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CocktailDebacle.Server.Models;
using CocktailDebacle.Server.Service;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System;
using Microsoft.AspNetCore.RateLimiting;
using CocktailDebacle.Server.Models.DTOs; // Importa il namespace del DTO

using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace CocktailDebacle.Server.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;

        private readonly ILogger<UsersController> _logger;

        public UsersController(AppDbContext context, IAuthService authService, ILogger<UsersController> logger)
        {
            _logger = logger;
            _logger.LogInformation("UsersController initialized.✅");
            _context = context;
            _authService = authService;
        }

        [HttpPost("login")]
        [EnableRateLimiting("fixed")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Trova l'utente corrispondente
            var user = await _context.DbUser
                .Where(u => u.UserName == request.UserNameRequest)
                .FirstOrDefaultAsync(); // Recupera l'utente completo, inclusa la password hashata

            if (user == null)
            {
                return NotFound("User not found");
            }

            // Verifica la password hashata con BCrypt
            bool passwordMatch = BCrypt.Net.BCrypt.Verify(request.PasswordRequest, user.PasswordHash);
            if (!passwordMatch)
            {
                return Unauthorized("Invalid password");
            }

            var token = await _authService.AuthenticateUser(user.UserName, request.PasswordRequest, user);
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized($"Invalid token = {token}");
            }
            _logger.LogDebug($"Token = {token}");
            user.Token = token;
            await _context.SaveChangesAsync();
            // Se la password è corretta, restituisci i dati utente
            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Name,
                user.LastName,
                user.Email,
                user.AcceptCookies,
                Token = token
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            var user = await _context.DbUser.FirstOrDefaultAsync(u => u.UserName == request.UserName);
            if (user == null)
                return NotFound($"Utente non trovato{request.UserName} = {user?.UserName}");

            user.Token = string.Empty;
            await _context.SaveChangesAsync();

            return new JsonResult(new { message = "Logout effettuato con successo." });
        }

        [HttpGet("check-token")]
        [Authorize]
        public Task<IActionResult> CheckToken()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.Identity?.Name;
            return Task.FromResult<IActionResult>(Ok(new
            {
                Message = "Token valido",
                UserId = userId,
                UserName = userName
            }));
        }

        [HttpGet("GetToken")]
        public async Task<IActionResult> GetToken(string userName)
        {
            var user = await _context.DbUser.FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null)
                return NotFound($"Utente non trovato{userName} = {user?.UserName}");

            return Ok(user.Token);
        }


        [HttpPost("register")]
        public async Task<ActionResult<User>> Register([FromBody] RegisterUserDto userDto)
        {
            // Controlla se esiste già un utente con la stessa email
            bool emailExists = await _context.DbUser.AnyAsync(u => u.Email == userDto.Email);
            bool userNameExists = await _context.DbUser.AnyAsync(u => u.UserName == userDto.UserName);
            if (userNameExists)
            {
                return BadRequest("Questo Nome Utente è già in uso.");
            }
            if (emailExists)
            {
                return BadRequest("Questa Email è già in uso?.");
            }

            // Mappa il DTO al modello User
            var user = new User
            {
                UserName = userDto.UserName,
                Name = userDto.Name,
                LastName = userDto.LastName,
                Email = userDto.Email,
                PasswordHash = HashPassword(userDto.PasswordHash),
                AcceptCookies = userDto.AcceptCookies
            };

            // Aggiungi l'utente al database
            _context.DbUser.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Login), new { id = user.Id }, user);
        }




        // PUT: api/Users/{id} - Modifica un utente esistente
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User updatedUser)
        {
            // Trova l'utente nel database
            var user = await _context.DbUser.FindAsync(id);
            if (user == null)
            {
                return NotFound("Utente non trovato.");
            }

            // Aggiorna solo i campi modificabili
            user.UserName = updatedUser.UserName;
            user.Name = updatedUser.Name;
            user.LastName = updatedUser.LastName;
            user.Email = updatedUser.Email;
            user.AcceptCookies = updatedUser.AcceptCookies;
            //user.Online = updatedUser.Online;
            // user.PersonalizedExperience = updatedUser.PersonalizedExperience;
            // user.Leanguage = updatedUser.Leanguage;
            // user.ImgProfile = updatedUser.ImgProfile;

            // Se la password viene cambiata, aggiorna l'hash
            if (!string.IsNullOrEmpty(updatedUser.PasswordHash) && updatedUser.PasswordHash != user.PasswordHash)
            {
                user.PasswordHash = HashPassword(updatedUser.PasswordHash);
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Errore durante l'aggiornamento dell'utente.");
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.DbUser.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            _context.DbUser.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

            // Metodo per l'hashing della password
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }

    public class LoginRequest
    {
        public string UserNameRequest { get; set; } = string.Empty;
        public string PasswordRequest { get; set; } = string.Empty;
    }

    public class LogoutRequest
    {
        public string UserName { get; set; } = string.Empty;
    }
}
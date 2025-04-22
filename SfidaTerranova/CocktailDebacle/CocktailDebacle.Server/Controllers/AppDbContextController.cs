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
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using CloudinaryDotNet;

namespace CocktailDebacle.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;

        private readonly CloudinaryService _cloudinaryService;

        private readonly ILogger<UsersController> _logger;

        public UsersController(AppDbContext context, IAuthService authService, ILogger<UsersController> logger, CloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
            _authService = authService;
            _context = context;
            _logger = logger;
            _logger.LogInformation("CloudinaryService initialized.✅");
        }


        // http://localhost:5052/api/Users/login + body -> row {"userNameRequest": ="" "passwordRequest": ""}
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


        // http://localhost:5052/api/Users/logout + body -> row {"userName": =""}
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


        // http://localhost:5052/api/Users/check-token
        [HttpGet("check-token")]
        [Authorize]
        public async Task<IActionResult> CheckToken()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.DbUser.FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user == null)
            {
                return Unauthorized("Utente non trovato");
            }

            if (string.IsNullOrEmpty(user.Token) || user.TokenExpiration < DateTime.UtcNow)
            {
                user.Token = string.Empty;
                user.TokenExpiration = null;
                await _context.SaveChangesAsync();
                return Unauthorized("Token non valido o scaduto");
            }

            return Ok(new
            {
                Message = "Token valido",
                UserId = userId,
                user.UserName
            });
        }

        // http://localhost:5052/api/Users/GetToken?userName=...
        [HttpGet("GetToken")]
        public async Task<IActionResult> GetToken(string userName)
        {
            var user = await _context.DbUser.FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null)
                return NotFound($"Utente non trovato{userName} = {user?.UserName}");
            return Ok(user.Token);
        }

        // http://localhost:5052/api/Users + body -> row {"userName": ="" "name": ="" "lastName": ="" "email": ="" "passwordHash": ="" "acceptCookies": =""}
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
        // http://localhost:5052/api/Users/1 + body -> row {"userName": ="" "name": ="" "lastName": ="" "email": ="" "passwordHash": ="" "acceptCookies": =""}
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

        // http://localhost:5052/api/Users/{id} - Elimina un utente
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


        /// IMG Profile ///

        // per qunado fai riferimento al Url della immagine dell'user guarda ache questo video per capire come personalizzarlo
        // https://www.youtube.com/watch?v=P4FhRuttCgY

        /// <summary>
        /// ✅ Upload di un'immagine profilo da file locale per un determinato utente.
        /// 🔒 L'immagine sarà caricata su Cloudinary (autenticata) nella cartella `profile_images/`.
        /// 🧼 Se l'utente ha già un'immagine, verrà eliminata.
        /// </summary>
        /// <remarks>
        /// 📥 Chiamata HTTP:
        ///     POST http://localhost:5052/api/Users/{UserName}/upload-profile-image-local
        ///
        /// 📦 Content-Type:
        ///     multipart/form-data
        ///
        /// 🔑 Parametri nel Body (form-data):
        ///     Key:    file
        ///     Value:  (il file immagine .png, .jpg, ecc...)
        ///
        /// 📤 Esempio Postman:
        ///     - Metodo: POST
        ///     - URL:    http://localhost:5052/api/Users/Vik8/upload-profile-image-local
        ///     - Body: form-data
        ///         ▸ Key: file (tipo = File)
        ///         ▸ Value: selezionare immagine dal disco
        /// </remarks>

        [HttpPost("{UserName}/upload-profile-image-local")]
        public async Task<IActionResult> UploadProfileImageLocal(string UserName, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Nessun file caricato.");
            }

            var user = await _context.DbUser.FirstOrDefaultAsync(u => u.UserName == UserName);
            
            if (user == null)
            {
                return NotFound("Utente non trovato.");
            }
            
            if (!string.IsNullOrEmpty(user.ImgProfileUrl))
            {
                try
                {
                    var uri = new Uri(user.ImgProfileUrl);
                    var segments = uri.AbsolutePath.Split('/');
                    var folder = string.Join("/", segments.Skip(segments.ToList().IndexOf("upload") + 1));
                    var publicId = Path.Combine(Path.GetDirectoryName(folder) ?? "", Path.GetFileNameWithoutExtension(folder)).Replace("\\", "/");

                    var result = await _cloudinaryService.DeleteImageAsync(publicId);
                    Console.WriteLine($"[Cloudinary] Eliminata: {publicId} → {result}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Errore durante l'eliminazione dell'immagine precedente: {ex.Message}");
                }
            }

            var publicIdNew = $"profile_images/{UserName}";
            var uploadedUrl = await _cloudinaryService.UploadImageAsync(file, publicIdNew);

            if (uploadedUrl == null)
                return BadRequest("Errore nel caricamento dell'immagine.");

            user.ImgProfileUrl = uploadedUrl;
            await _context.SaveChangesAsync();

            return Ok(new { Url = uploadedUrl });
        }

        /// <summary>
        /// ✅ Caricamento immagine profilo da URL per un determinato utente.
        /// 📤 L'immagine sarà scaricata dal link fornito e ricaricata su Cloudinary nella cartella `profile_images/`.
        /// 🧼 Se l'utente ha già un'immagine, verrà eliminata automaticamente.
        /// </summary>
        /// <remarks>
        /// 📥 Chiamata HTTP:
        ///     POST http://localhost:5052/api/Users/{UserName}/upload-profile-image-Url
        ///
        /// 📦 Content-Type:
        ///     application/json
        ///
        /// 🧾 Body (raw, JSON):
        ///     "https://example.com/image.jpg"
        ///
        /// 📤 Esempio Postman:
        ///     - Metodo: POST  
        ///     - URL:    http://localhost:5052/api/Users/Vik8/upload-profile-image-Url  
        ///     - Headers:  
        ///         ▸ Content-Type: application/json  
        ///     - Body: raw → JSON  
        ///         "https://images.miosito.com/profile.jpg"
        /// </remarks>
        [HttpPost("{UserName}/upload-profile-image-Url")]
        public async Task<IActionResult> UploadProfileImageUrl(string UserName, [FromBody] string ImgProfileUrl)
        {
           var user = await _context.DbUser.FirstOrDefaultAsync(u => u.UserName == UserName);
            
            if (user == null)
            {
                return NotFound("Utente non trovato.");
            }

            if (!string.IsNullOrEmpty(user?.ImgProfileUrl))
            {
                try
                {
                    var uri = new Uri(user.ImgProfileUrl);
                    var segments = uri.AbsolutePath.Split('/');
                    var folder = string.Join("/", segments.Skip(segments.ToList().IndexOf("upload") + 1));
                    var publicId = Path.Combine(Path.GetDirectoryName(folder) ?? "", Path.GetFileNameWithoutExtension(folder)).Replace("\\", "/");
                    await _cloudinaryService.DeleteImageAsync(publicId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Errore durante l'eliminazione dell'immagine precedente: {ex.Message}");
                    // Non bloccare il flusso
                }
            }
            
            var newPublicId = $"profile_images/{UserName}";
            var uploadedUrl = await _cloudinaryService.UploadImageAsyncUrl(ImgProfileUrl, newPublicId);
            if (uploadedUrl == null)
                return BadRequest("Errore nel caricamento dell'immagine.");
            if(user?.ImgProfileUrl != null)
                user.ImgProfileUrl = uploadedUrl;
            
            await _context.SaveChangesAsync();

            return Ok(new { Url = uploadedUrl });
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
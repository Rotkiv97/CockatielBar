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
using CocktailDebacle.Server.DTOs;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using CloudinaryDotNet;
using CocktailDebacle.Server.Utils;

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

        [Authorize]
        [HttpGet("GetUser/{username}")]
        public async Task<ActionResult<UserDto>> GetUser(string username)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Utente non autenticato.");
            }
            var user = await _context.DbUser.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
            {
                return NotFound("Utente non trovato.");
            }

            var userDto = UtilsUserController.UserToDto(user); 
            return Ok(userDto);
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
            //user.Token = token;
            //user.TokenExpiration = DateTime.UtcNow; // Imposta la scadenza del token a 1 ora
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
        public async Task<IActionResult> CheckToken(string userName)
        {
            var user = await _context.DbUser.FirstOrDefaultAsync(u => u.UserName == userName);

            if (user == null)
            {
                return Unauthorized("Utente non trovato");
            }
            // Token mancante o TokenExpiration nullo => scaduto
            if (string.IsNullOrEmpty(user.Token) || user.TokenExpiration == null)
            {
                user.Token = string.Empty;
                user.TokenExpiration = null;
                await _context.SaveChangesAsync();
                return Unauthorized("Token assente o scaduto");
            }

            // Token presente ma scaduto nel tempo
            if (user.TokenExpiration < DateTime.UtcNow)
            {
                user.Token = string.Empty;
                user.TokenExpiration = null;
                await _context.SaveChangesAsync();
                return Unauthorized("Token scaduto");
            }

            return Ok(new
            {
                Message = "Token valido",
                UserName = userName,
            });
        }

        // http://localhost:5052/api/Users/GetToken?userName=...
        [HttpGet("GetToken")]
        public async Task<IActionResult> GetToken(string userName)
        {
            var user = await _context.DbUser.FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null)
                return NotFound($"Utente non trovato{userName} = {user?.UserName}");
            return Ok( new {token = user.Token.ToString()});
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
                IsOfMajorityAge = userDto.IsOfMajorityAge,
                AcceptCookies = userDto.AcceptCookies
            };

            // Aggiungi l'utente al database
            _context.DbUser.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Login), new { id = user.Id }, user);
        }




        // PUT: api/Users/{id} - Modifica un utente esistente
        // http://localhost:5052/api/Users/1 + body -> row {"userName": ="" "name": ="" "lastName": ="" "email": ="" "passwordHash": ="" "acceptCookies": =""}
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User updatedUser)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
                return Unauthorized("Utente non autenticato.");
            var user = await _context.DbUser.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return NotFound("Utente non trovato.");
            if (user.UserName != userName)
                return Forbid("Non puoi modificare altri utenti.");

            if (user.UserName != updatedUser.UserName)
            {
                var userNameExists = await _context.DbUser.AnyAsync(u => u.UserName == updatedUser.UserName && u.Id != id);
                if (userNameExists)
                    return BadRequest("Questo Nome Utente è già in uso.");
                user.UserName = updatedUser.UserName ?? string.Empty;
            }
            if (user.Name != updatedUser.Name)
                user.Name = updatedUser.Name ?? string.Empty;

            if (user.LastName != updatedUser.LastName)
                user.LastName = updatedUser.LastName ?? string.Empty;

            if (user.Email != updatedUser.Email)
            {
                var emailExists = await _context.DbUser.AnyAsync(u => u.Email == updatedUser.Email && u.Id != id);
                if (emailExists)
                    return BadRequest("Questa email è già in uso.");
                user.Email = updatedUser.Email ?? string.Empty;
            }
            if (user.AcceptCookies != updatedUser.AcceptCookies)
            {
                user.AcceptCookies = updatedUser.AcceptCookies;

                if (user.AcceptCookies == false)
                {
                    var history = await _context.DbUserHistorySearch
                        .Where(h => h.UserId == user.Id)
                        .ToListAsync();
                    _context.DbUserHistorySearch.RemoveRange(history);
                }
            }

            if (!string.IsNullOrEmpty(updatedUser.PasswordHash) && updatedUser.PasswordHash != user.PasswordHash)
                user.PasswordHash = HashPassword(updatedUser.PasswordHash);

            string? newToken = null;
            try
            {
                newToken = await _authService.AuthenticateUser(user.UserName, user.PasswordHash, user);
                user.Token = newToken;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Errore durante l'aggiornamento dell'utente.");
            }
            var userDto = UtilsUserController.UserToDto(user);
            var newUser = new 
            {
                userDto.Id,
                userDto.UserName,
                userDto.Name,
                userDto.LastName,
                userDto.Email,
                acceptCookies = user.AcceptCookies,
                PasswordHash = user.PasswordHash,
                Token = newToken
            };
            return Ok(newUser);
        }

        [Authorize]
        [HttpGet("getPassword/{id}")]
        public async Task<IActionResult> getPassword(int id)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
                return Unauthorized("Utente non autenticato.");
            var user = await _context.DbUser.FindAsync(id);
            if (user == null)
                return NotFound("Utente non trovato.");
            var password = user.PasswordHash;
            if (string.IsNullOrEmpty(password))
                return NotFound("Password non trovata.");
            return Ok(new { Password = password });
        }
        // http://localhost:5052/api/Users/{id} - Elimina un utente
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Utente non autenticato.");
            }
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
        [Authorize]
        [HttpPost("{UserName}/upload-profile-image-local")]
        public async Task<IActionResult> UploadProfileImageLocal(string UserName, IFormFile file)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Utente non autenticato.");
            }
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

        [Authorize]
        [HttpGet("GetMyCocktailLike")]
        public async Task<IActionResult> GetMyCocktailLike()
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Utente non autenticato.");
            }

            var user = await _context.DbUser.FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null)
            {
                return NotFound("Utente non trovato.");
            }

            var cocktailLike = await _context.DbCocktails
                .Where(c => c.UserLikes.Any(u => u.UserName == userName))
                .ToListAsync();
            if (cocktailLike == null || cocktailLike.Count == 0)
            {
                return NotFound("Nessun cocktail trovato.");
            }
            var cocktailDtos = cocktailLike.Select(c => UtilsCocktail.CocktailToDto(c)).ToList();

            if(!cocktailDtos.Any())
            {
                return NotFound("Nessun cocktail trovato.");
            }
            return Ok(cocktailDtos);
        }

        [Authorize]
        [HttpPost("FollowedNewUser/{followedUserId}")]
        public async Task<IActionResult> FollowedNewUser(int followedUserId)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
                return Unauthorized("Utente non autenticato.");

            var user = await _context.DbUser
                .Include(u => u.Followed_Users)
                .FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null)
                return NotFound("Utente non trovato.");

            var followedUser = await _context.DbUser
                .Include(u => u.Followers_Users)
                .FirstOrDefaultAsync(u => u.Id == followedUserId);
            if (followedUser == null)
                return NotFound("Utente seguito non trovato.");
            
            bool FollowingUser = user.Followed_Users.Any(u => u.Id == followedUser.Id);
            if(followedUserId == user.Id)
            {
                return BadRequest("Non puoi seguire te stesso.");
            }
            else if (FollowingUser)
            {
                user.Followed_Users.Remove(followedUser);
                followedUser.Followers_Users.Remove(user);
                await _context.SaveChangesAsync();
                return Ok(new { Message = $"Non segui più questo utente = {followedUserId}" });
            }
            else
            {
                user.Followed_Users.Add(followedUser);
                followedUser.Followers_Users.Add(user);
                await _context.SaveChangesAsync();
                return Ok(new { Message = $"Ora segui questo utente = {followedUserId}" });
            }
        }


        [Authorize]
        [HttpGet("GetFollowedUsers/{id}")]
        public async Task<IActionResult> GetFollowedUsers(int id)
        {
            if(id <= 0)
                return BadRequest("ID non valido.");
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Utente non autenticato.");
            }

            var user = await _context.DbUser
                .Include(u => u.Followed_Users).FirstAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound("Utente non trovato.");
            }

            var followedUsers = user?.Followed_Users.Select(UtilsUserController.UserToDto).ToList();

            return Ok(followedUsers);
        }

        [Authorize]
        [HttpGet("GetFollowersUsers/{id}")]
        public async Task<IActionResult> GetFollowersUsers(int id)
        {
            if (id <= 0)
                return BadRequest("ID non valido.");

            var userName = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Utente non autenticato.");
            }

            var user = await _context.DbUser
                .Include(u => u.Followers_Users)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound("Utente non trovato.");
            }

            var followersUsers = user.Followers_Users.Select(UtilsUserController.UserToDto).ToList();
            return Ok(followersUsers);
        }

        [Authorize]
        [HttpGet("Get_Cocktail_for_Followed_Users")] // api per ottenere i cocktail degli utenti seguiti
        public async Task<IActionResult> GetCocktailForFollowedUsers()
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Utente non autenticato.");
            }

            var user = await _context.DbUser
                .Include(u => u.Followed_Users)
                .ThenInclude(f => f.CocktailsLike)
                .FirstOrDefaultAsync(u => u.UserName == userName);

            if (user == null)
            {
                return NotFound("Utente non trovato.");
            }

            var cocktailDtos = user.Followed_Users.SelectMany(f => f.CocktailsLike)
                .Select(c => new CocktailDto
                {
                    Id = c.Id,
                    IdDrink = c.IdDrink ?? string.Empty,
                    StrDrink = c.StrDrink ?? string.Empty,
                    StrCategory = c.StrCategory ?? string.Empty,
                    StrAlcoholic = c.StrAlcoholic ?? string.Empty,
                    StrGlass = c.StrGlass ?? string.Empty,
                    StrInstructions = c.StrInstructions ?? string.Empty,
                    StrDrinkThumb = c.StrDrinkThumb ?? string.Empty,
                    Ingredients = UtilsCocktail.IngredientToList(c),
                    Measures = UtilsCocktail.MeasureToList(c),
                    StrTags = c.StrTags ?? string.Empty
                }).ToList();
            if (!cocktailDtos.Any())
            {
                return NotFound("Nessun cocktail trovato.");
            }
            return Ok(cocktailDtos);
        }

        // http://localhost:5052/api/Users/ThisYourCocktailLike/{id}
        [HttpGet("ThisYourCocktailLike/{id}")]
        public async Task<IActionResult> ThisYourCocktailLike(int id)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Utente non autenticato.");
            }

            var user = await _context.DbUser.FirstAsync(u => u.UserName == userName);
            if (user == null)
            {
                return NotFound("Utente non trovato.");
            }
            var cocktail = await _context.DbCocktails
                .Include(c => c.UserLikes)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cocktail == null)
            {
                return NotFound("Cocktail non trovato.");
            }
            var isLiked = cocktail.UserLikes.Any(u => u.UserName == userName);
            return Ok(isLiked);
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
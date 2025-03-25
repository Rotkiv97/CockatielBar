using Microsoft.AspNetCore.Mvc;
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
using CocktailDebacle.Server.Models.DTOs;
using Microsoft.AspNetCore.Authorization; // Importa il namespace del DTO


namespace CocktailDebacle.Server.Controllers
{
    [Route("api/Cocktaildebacle")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly AppDbContext _context;
        public UsersController(AppDbContext dbcontext, IAuthService authService)
        {
            _context = dbcontext;
            _authService = authService;
        }
        // COCKTAILS /////////////////////////////////////////////////////


        [HttpGet("all-cocktails")]
        public async Task<ActionResult<IEnumerable<Cocktail>>> GetAllCocktails()
        {
            var cocktails = await _context.DbCocktails.Include(c => c.Ingredients).ToListAsync();
            return Ok(cocktails);
        }

        [HttpGet("cocktail/{id}")]
        public async Task<ActionResult<Cocktail>> GetCocktail(int id)
        {
            var cocktail = await _context.DbCocktails.Include(c => c.Ingredients).FirstOrDefaultAsync(c => c.Id == id);
            if (cocktail == null)
            {
                return NotFound();
            }
            return Ok(cocktail);
        }


        // USER /////////////////////////////////////////////////////

        // GET: api/Users - Restituisce tutti gli utenti
        [HttpGet("all-users")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.DbUser.ToListAsync();
        }

        // GET: api/Users/{id} - Restituisce un utente specifico        
        [Authorize] // proteggere l'endpoint con JWT
        [HttpGet("user")]
        public async Task<ActionResult<User>> GetUserProfile()
        {
            var UserName = User.Identity?.Name;
            if(string.IsNullOrEmpty(UserName))
            {
                return NotFound();
            }
            var user = await _context.DbUser
                .Where(u => u.UserName == UserName)
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Name,
                    u.LastName,
                    u.Email,
                    //u.PersonalizedExperience,
                    u.AcceptCookies,
                    //u.Online,
                    //u.Language,
                    //u.ImgProfile
                })
                .FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }


        [HttpPost("login")]
        [EnableRateLimiting("fixed")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var token = await _authService.AuthenticateUser(request.UserNameRequest, request.PasswordRequest);

            if (token == null)
            {
                return Unauthorized(new { message = "Credenziali non valide" });
            }

            // Trova l'utente corrispondente
            var user = await _context.DbUser
                .Where(u => u.UserName == request.UserNameRequest)
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Name,
                    u.LastName,
                    u.Email,
                   // u.PersonalizedExperience,
                    u.AcceptCookies,
                   // u.Online,
                   // u.Language,
                  //  u.ImgProfile
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("Utente non trovato.");
            }

            return Ok(new { token, user });
        }

            // POST: api/Users/register - Aggiunge un nuovo utente a Users
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register([FromBody] RegisterUserDto userDto)
        {
            // Controlla se esiste già un utente con la stessa email
            bool emailExists = await _context.DbUser.AnyAsync(u => u.Email == userDto.Email);
            if (emailExists)
            {
                return BadRequest("Questa Email è già in uso.");
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
                using (var sha256 = SHA256.Create())
                {
                    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    return Convert.ToBase64String(bytes);
                }
            }
        }

        public class LoginRequest
        {
            public string UserNameRequest { get; set; } = string.Empty;
            public string PasswordRequest { get; set; } = string.Empty;
        }
}

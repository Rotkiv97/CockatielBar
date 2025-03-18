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

namespace CocktailDebacle.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;

        public UsersController(AppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Users>>> GetUsers()
        {
            return await _context.DbUsers.Include(u => u.UserList).ToListAsync();
        }

        // GET: api/Users/5 - Restituisce un utente in base alla passsword e all'email
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var token = await _authService.AuthenticateUser(request.EmailRequest, request.PasswordRequest);

            if (token == null)
            {
                return Unauthorized(new { message = "Credenziali non valide" });
            }

            // Trova l'utente corrispondente
            var user = await _context.DbUser
                .Where(u => u.Email == request.EmailRequest)
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Name,
                    u.LastName,
                    u.Email,
                    u.PersonalizedExperience,
                    u.AcceptCookis,
                    u.Online,
                    u.Leanguage,
                    u.ImgProfile
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("Utente non trovato.");
            }

            return Ok(new { token, user });
        }

        // POST: api/Users/register - Aggiunge un nuovo utente a Users
        [HttpPost("register/{usersId}")]
        public async Task<ActionResult<User>> Register(int usersId, User user)
        {
            var users = await _context.DbUsers.FindAsync(usersId);
            if (users == null)
            {
                return NotFound("Users non trovato.");
            }

            if (await _context.DbUser.AnyAsync(u => u.Email == user.Email))
            {
                return BadRequest("Questa Email è già in uso");
            }

            user.PasswordHash = HashPassword(user.PasswordHash);
            user.UsersId = usersId; // Assegna l'utente a questo Users

            _context.DbUser.Add(user);
            users.UserList.Add(user);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Login), new { id = users.Id }, user);
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
        user.PersonalizedExperience = updatedUser.PersonalizedExperience;
        user.AcceptCookis = updatedUser.AcceptCookis;
        user.Online = updatedUser.Online;
        user.Leanguage = updatedUser.Leanguage;
        user.ImgProfile = updatedUser.ImgProfile;

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


        // DELETE: api/Users/5
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
        public string EmailRequest { get; set; } = string.Empty;
        public string PasswordRequest { get; set; } = string.Empty;
    }
}

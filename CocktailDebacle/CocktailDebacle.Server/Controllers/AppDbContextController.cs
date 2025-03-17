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

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Users>>> GetUsers()
        {
            return await _context.DbUsers.Include(u => u.UserList).ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Users>> GetUsersById(int id)
        {
            var users = await _context.DbUsers
                                      .Include(u => u.UserList)
                                      .FirstOrDefaultAsync(u => u.Id == id);

            if (users == null)
            {
                return NotFound();
            }

            return Ok(users);
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

            user.Password = HashPassword(user.Password);
            user.UsersId = usersId; // Assegna l'utente a questo Users

            _context.DbUser.Add(user);
            users.UserList.Add(user);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsersById), new { id = users.Id }, user);
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
        if (!string.IsNullOrEmpty(updatedUser.Password) && updatedUser.Password != user.Password)
        {
            user.Password = HashPassword(updatedUser.Password);
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
}

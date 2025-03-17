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
            var users = await Task.Run(() =>
            {
                List<Users> result = new List<Users>();
                foreach (var user in _context.Users)
                {
                    if (user != null)
                    {
                        result.Add(user);
                    }
                }
                return result;
            });

            return users;
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        //registrazione 
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(User user)
        {
            if (await _context.UserList.AnyAsync(u => u.Email == user.Email)) {
                return BadRequest("Questa Email è gia in uso");
            }

            user.Password = HashPassword(user.Password);
            _context.UserList.Add(user);
            await _context.SaveChangesAsync();
            // gli Id vengono gestiti automaticamente da Entity Framework Core
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new
            {
                user.Id,
                user.Name,
                user.LastName,
                user.Email,
                user.Password
            });
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }


        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            _context.UserList.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // PUT: api/Users/5 // aggiornamento dell'user
        // Questa funzione è un endpoint API in ASP.NET Core che gestisce la modifica (aggiornamento)
        // di un utente esistente nel database
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User Upuser)
        {
           var user = await _context.UserList.FindAsync(id);

            // salviamo le modifiche nel database
            await _context.SaveChangesAsync();
            // Aggiorna solo i campi modificabili
            user.Name = Upuser.Name;
            user.LastName = Upuser.LastName;
            user.Email = Upuser.Email;
            user.PersonalizedExperience = Upuser.PersonalizedExperience;
            user.AcceptCookis = Upuser.AcceptCookis;
            user.Leanguage = Upuser.Leanguage;
            user.ImgProfile = Upuser.ImgProfile;
            if (!string.IsNullOrEmpty(Upuser.Password) && Upuser.Password != user.Password)
            {
                user.Password = HashPassword(Upuser.Password);
            }
            return NoContent();
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

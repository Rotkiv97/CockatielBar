﻿﻿﻿using Microsoft.AspNetCore.Mvc;
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
        }

        [Authorize]
        [HttpGet("GetUser/{username}")]
        public async Task<ActionResult<UserDto>> GetUser(string username)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Unauthenticated user.");
            }
            var user = await _context.DbUser.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var userDto = UtilsUserController.UserToDto(user); 
            return Ok(userDto);
        }

        [HttpPost("login")]
        [EnableRateLimiting("fixed")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.DbUser
                .Where(u => u.UserName == request.UserNameRequest)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Verifico la password hashata con BCrypt
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
            await _context.SaveChangesAsync();
            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Name,
                user.LastName,
                user.Email,
                user.AcceptCookies,
                Token = token,
                ImgProfileUrl = user.ImgProfileUrl ?? string.Empty,
                Bio = user.Bio ?? string.Empty,
                Bio_link = user.Bio_link ?? string.Empty,
                Language = user.Language ?? string.Empty
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            var user = await _context.DbUser.FirstOrDefaultAsync(u => u.UserName == request.UserName);
            if (user == null)
                return NotFound($"User not found{request.UserName} = {user?.UserName}");

            user.Token = string.Empty;
            await _context.SaveChangesAsync();

            return new JsonResult(new { message = "Logged out successfully." });
        }

        [HttpGet("check-token")]
        public async Task<IActionResult> CheckToken(string userName)
        {
            var user = await _context.DbUser.FirstOrDefaultAsync(u => u.UserName == userName);

            if (user == null)
            {
                return Unauthorized("Unauthenticated user.");
            }
            // Token mancante o TokenExpiration nullo => scaduto
            if (string.IsNullOrEmpty(user.Token) || user.TokenExpiration == null)
            {
                user.Token = string.Empty;
                user.TokenExpiration = null;
                await _context.SaveChangesAsync();
                return Unauthorized("Token missing or expired");
            }

            // Token presente ma scaduto nel tempo
            if (user.TokenExpiration < DateTime.UtcNow)
            {
                user.Token = string.Empty;
                user.TokenExpiration = null;
                await _context.SaveChangesAsync();
                return Unauthorized("Token expired");
            }

            return Ok(new
            {
                Message = "Valid token",
                UserName = userName,
            });
        }

        [HttpGet("GetToken")]
        public async Task<IActionResult> GetToken(string userName)
        {
            var user = await _context.DbUser.FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null)
                return NotFound($"Utente not found {userName} = {user?.UserName}");
            return Ok( new {token = user.Token.ToString()});
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register([FromBody] RegisterUserDto userDto)
        {
            bool emailExists = await _context.DbUser.AnyAsync(u => u.Email == userDto.Email);
            bool userNameExists = await _context.DbUser.AnyAsync(u => u.UserName == userDto.UserName);

            if (userNameExists)
            {
                return BadRequest("This Username is already in use.");
            }
            if (emailExists)
            {
                return BadRequest("Is this email already in use?.");
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

            _context.DbUser.Add(user);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Login), new { id = user.Id }, user);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User updatedUser)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
                return Unauthorized("Unauthenticated user.");
            var user = await _context.DbUser.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return NotFound("User not found.");
            if (user.UserName != userName)
                return Forbid("You cannot edit other users.");

            if (user.UserName != updatedUser.UserName)
            {
                var userNameExists = await _context.DbUser.AnyAsync(u => u.UserName == updatedUser.UserName && u.Id != id);
                if (userNameExists)
                    return BadRequest("This Username is already in use.");
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
                    return BadRequest("This email is already in use.");
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
                return StatusCode(500, "Error updating user.");
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
                return Unauthorized("Unauthenticated user.");
            var user = await _context.DbUser.FindAsync(id);
            if (user == null)
                return NotFound("User not found.");
            var password = user.PasswordHash;
            if (string.IsNullOrEmpty(password))
                return NotFound("Password not found.");
            return Ok(new { Password = password });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Unauthenticated user.");
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

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        [Authorize]
        [HttpPost("upload-profile-image-local/{id}")]
        public async Task<IActionResult> UploadProfileImageLocal(int id, IFormFile file)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Unauthenticated user.");
            }
            if (file == null || file.Length == 0)
            {
                return BadRequest("No files loaded.");
            }

            var user = await _context.DbUser.FirstOrDefaultAsync(u => u.Id == id);
            
            if (user == null)
            {
                return NotFound("User not found.");
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
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting previous image: {ex.Message}");
                }
            }

            var publicIdNew = $"profile_images/{id}";
            var uploadedUrl = await _cloudinaryService.UploadImageAsync(file, publicIdNew);

            if (uploadedUrl == null)
                return BadRequest("Error loading image.");

            user.ImgProfileUrl = uploadedUrl;
            await _context.SaveChangesAsync();

            return Ok(new { Url = uploadedUrl });
        }

        [Authorize]
        [HttpPost("upload-profile-image-Url/{id}")]
        public async Task<IActionResult> UploadProfileImageUrl(int id, [FromBody] string ImgProfileUrl)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Unauthenticated user.");
            }
            if (string.IsNullOrEmpty(ImgProfileUrl))
            {
                return BadRequest("No URL provided.");
            }
           var user = await _context.DbUser.FirstOrDefaultAsync(u => u.Id== id);
            
            if (user == null)
            {
                return NotFound("User not found.");
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
                    Console.WriteLine($"Error deleting previous image: {ex.Message}");
                }
            }
            
            var newPublicId = $"profile_images/{id}";
            var uploadedUrl = await _cloudinaryService.UploadImageAsyncUrl(ImgProfileUrl, newPublicId);
            if (uploadedUrl == null)
                return BadRequest("Error loading image.");
            if(user?.ImgProfileUrl != null)
                user.ImgProfileUrl = uploadedUrl;
            
            await _context.SaveChangesAsync();

            return Ok(new { Url = uploadedUrl });
        }

        [Authorize]
        [HttpGet("GetMyCocktailLike/{id}")]
        public async Task<IActionResult> GetMyCocktailLike(int id)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Unauthenticated user.");
            }

            var user = await _context.DbUser.FirstAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var cocktailLike = await _context.DbCocktails
                .Where(c => c.UserLikes.Any(u => u.Id == user.Id))
                .ToListAsync();
            if (cocktailLike == null || cocktailLike.Count == 0)
            {
                return NotFound("No cocktails found.");
            }
            var cocktailDtos = cocktailLike.Select(c => UtilsCocktail.CocktailToDto(c)).ToList();

            if(!cocktailDtos.Any())
            {
                return NotFound("No cocktails found.");
            }
            return Ok(cocktailDtos);
        }

        [Authorize]
        [HttpPost("FollowedNewUser/{followedUserId}")]
        public async Task<IActionResult> FollowedNewUser(int followedUserId)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
                return Unauthorized("Unauthenticated user.");

            var user = await _context.DbUser
                .Include(u => u.Followed_Users)
                .FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null)
                return NotFound("User not found.");

            var followedUser = await _context.DbUser
                .Include(u => u.Followers_Users)
                .FirstOrDefaultAsync(u => u.Id == followedUserId);
            if (followedUser == null)
                return NotFound("Followed user not found.");
            
            bool FollowingUser = user.Followed_Users.Any(u => u.Id == followedUser.Id);
            if(followedUserId == user.Id)
            {
                return BadRequest("You can't follow yourself.");
            }
            else if (FollowingUser)
            {
                user.Followed_Users.Remove(followedUser);
                followedUser.Followers_Users.Remove(user);
                await _context.SaveChangesAsync();
                return Ok(new { Message = $"You no longer follow this user = {followedUserId}" });
            }
            else
            {
                user.Followed_Users.Add(followedUser);
                followedUser.Followers_Users.Add(user);
                await _context.SaveChangesAsync();
                return Ok(new { Message = $"Now follow this user = {followedUserId}" });
            }
        }


        [Authorize]
        [HttpGet("GetFollowedUsers/{id}")]
        public async Task<IActionResult> GetFollowedUsers(int id)
        {
            if(id <= 0)
                return BadRequest("Invalid ID.");
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Unauthenticated user.");
            }

            var user = await _context.DbUser
                .Include(u => u.Followed_Users).FirstAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var followedUsers = user?.Followed_Users.Select(UtilsUserController.UserToDto).ToList();

            return Ok(followedUsers);
        }

        [Authorize]
        [HttpGet("GetFollowersUsers/{id}")]
        public async Task<IActionResult> GetFollowersUsers(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid ID.");

            var userName = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Unauthenticated user.");
            }

            var user = await _context.DbUser
                .Include(u => u.Followers_Users)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var followersUsers = user.Followers_Users.Select(UtilsUserController.UserToDto).ToList();
            return Ok(followersUsers);
        }

        [Authorize]
        [HttpGet("Get_Cocktail_for_Followed_Users")]
        public async Task<IActionResult> GetCocktailForFollowedUsers()
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Unauthenticated user.");
            }

            var user = await _context.DbUser
                .Include(u => u.Followed_Users)
                .ThenInclude(f => f.CocktailsLike)
                .FirstOrDefaultAsync(u => u.UserName == userName);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var cocktailDtos = user.Followed_Users.SelectMany(f => f.CocktailsLike).Select(c => UtilsCocktail.CocktailToDto(c)).ToList();
            if (!cocktailDtos.Any())
            {
                return NotFound("No cocktails found.");
            }
            return Ok(cocktailDtos);
        }

        [HttpGet("ThisYourCocktailLike/{id}")]
        public async Task<IActionResult> ThisYourCocktailLike(int id)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Unauthenticated user.");
            }

            var user = await _context.DbUser.FirstAsync(u => u.UserName == userName);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            var cocktail = await _context.DbCocktails
                .Include(c => c.UserLikes)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cocktail == null)
            {
                return NotFound("Cocktail not found.");
            }
            var isLiked = cocktail.UserLikes.Any(u => u.UserName == userName);
            return Ok(isLiked);
        }

        [AllowAnonymous]
        [HttpGet("SuggestionsCocktailByUser/{id}")]
        public async Task<ActionResult<IEnumerable<object>>> GetSuggestions(
            int id,
            [FromQuery] string type = "likes",
            [FromQuery] int pageSize = 10
        )
        {
            var isLoggedIn = User.Identity?.IsAuthenticated ?? false;
            User? user = null;
            if (isLoggedIn)
            {
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;
                if (!string.IsNullOrEmpty(userName))
                {
                    user = await _context.DbUser
                        .Include(u => u.CocktailsLike)
                        .FirstOrDefaultAsync(u => u.Id == id);
                }
            }
            if (!isLoggedIn || id <= 0 || user == null)
            {
                var commonCategory = await _context.DbCocktails
                    .Where(c => !string.IsNullOrEmpty(c.StrCategory) && !string.IsNullOrEmpty(c.StrAlcoholic) && c.StrAlcoholic.ToLower() != "alcoholic")
                    .GroupBy(c => c.StrCategory)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .FirstOrDefaultAsync();

                var fallbackCocktails = await _context.DbCocktails
                    .Where(c => c.StrCategory == commonCategory && c.StrAlcoholic != null && c.StrAlcoholic.ToLower() != "alcoholic")
                    .OrderBy(c => Guid.NewGuid())
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(fallbackCocktails);
            }

            bool isAdult = user.IsOfMajorityAge ?? false;
            var likedCocktailIds = user.CocktailsLike.Select(c => c.Id).ToHashSet();

            IQueryable<Cocktail> query = _context.DbCocktails
                .Where(c => !likedCocktailIds!.Contains(c.Id) && (isAdult || c.StrAlcoholic!.ToLower() != "alcoholic"));

            List<Cocktail> results = new();

            if (type == "likes")
            {
                var likedCocktails = await _context.DbCocktails
                    .Where(c => c.UserLikes.Any(u => u.Id == user.Id))
                    .ToListAsync();

                if (likedCocktails.Count > 0)
                {
                    var similar = await UtilsUserController.FindSimilarCocktails(likedCocktails, query.ToList(), pageSize);
                    results.AddRange(similar);
                }
            }
            else if (type == "search")
            {
                var history = await UtilsUserController.GetSearchHistory(_context, user.Id);

                if (history.Any())
                {
                    var similar = await UtilsUserController.FindSimilarCocktails(history, query.ToList(), pageSize);
                    results.AddRange(similar);
                }
                else
                {
                    // fallback se history vuota
                    var fallback = await query
                        .OrderBy(_ => Guid.NewGuid())
                        .Take(pageSize)
                        .ToListAsync();
                    results.AddRange(fallback);
                }
            }
            else if (type == "category")
            {
                var category = await UtilsUserController.GetPreferredCategory(_context, user.Id);
                if (!string.IsNullOrEmpty(category))
                {
                    results.AddRange(await query
                        .Where(c => c.StrCategory == category)
                        .OrderBy(r => Guid.NewGuid())
                        .Take(pageSize)
                        .ToListAsync());
                }
            }

            if (results.Count < pageSize)
            {
                var additional = await query
                    .Where(c => !results.Select(r => r.Id).Contains(c.Id))
                    .OrderBy(r => Guid.NewGuid())
                    .Take(pageSize - results.Count)
                    .ToListAsync();

                results.AddRange(additional);
            }

            return Ok(results);
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
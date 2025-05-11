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
using CocktailDebacle.Server.Utils;
using CocktailDebacle.Server.DTOs;

using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.ComponentModel.DataAnnotations;

namespace CocktailDebacle.Server.Controllers
{
    [Route("api/[controller]")]
    public class CocktailsController : ControllerBase
    {
       
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;

        private readonly CloudinaryService _cloudinaryService; // Aggiungi questa riga per il servizio Cloudinary
        public CocktailsController(AppDbContext context, HttpClient httpClient, CloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
            _httpClient = httpClient;
        }


        // http://localhost:5052/api/Cocktails/cocktails
        [HttpGet("cocktails")]
        public async Task<ActionResult<IEnumerable<CocktailDto>>> GetCocktailsList()
        {
            var cocktails = await _context.DbCocktails
                .Select(c => UtilsCocktail.CocktailToDto(c)).ToListAsync();
            return Ok(cocktails);
        }


        //http://localhost:5052/api/Cocktails/cocktail/by-id?id=5
        [HttpGet("cocktail/by-id")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCocktailById(int id)
        {
            var cocktailEntity = await _context.DbCocktails
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync();

            if (cocktailEntity == null)
                return NotFound("Cocktail not found.");

            var cocktail = UtilsCocktail.CocktailToDto(cocktailEntity);

            if (User.Identity?.IsAuthenticated == true)
            {
                var userNameFromToken = User.FindFirst(ClaimTypes.Name)?.Value;
                if (!string.IsNullOrEmpty(userNameFromToken))
                {
                    var user = await _context.DbUser
                        .FirstOrDefaultAsync(u => u.UserName == userNameFromToken && u.AcceptCookies == true);

                    if (user != null && !string.IsNullOrEmpty(cocktail?.StrDrink))
                    {
                        bool exists = await _context.DbUserHistorySearch
                            .AnyAsync(h => h.UserId == user.Id && h.SearchText == cocktail.StrDrink);

                        if (!exists)
                        {
                            _context.DbUserHistorySearch.Add(new UserHistorySearch
                            {
                                UserId = user.Id,
                                SearchText = cocktail.StrDrink,
                                SearchDate = DateTime.UtcNow
                            });

                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }

            return Ok(cocktail);
        }




        // chiamata ipa e come impostarla 
        // http://localhost:5052/api/cocktails/search?ingredient=vodka&glass=Martini glass&alcoholic=Alcoholic&page=1&pageSize=10
        [AllowAnonymous]
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<CocktailDto>>> SearchCoctktail(
            [FromQuery] string nameCocktail = "",
            [FromQuery] string UserSearch = "",
            [FromQuery] string glass = "",
            [FromQuery] string ingredient = "",
            [FromQuery] string category = "",
            [FromQuery] string alcoholic = "",

            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
        {
            IQueryable<Cocktail> query = _context.DbCocktails.AsQueryable();
            List<Cocktail> cocktailListIngredient = new List<Cocktail>();
            var isAdult = false;
            int totalItems = 0;
            int totalPages = 0; 
            if (User.Identity?.IsAuthenticated == true)
            {
                var userNameFromToken = User.FindFirst(ClaimTypes.Name)?.Value;
                if (!string.IsNullOrEmpty(userNameFromToken))
                {
                    var usertmp = await _context.DbUser
                        .FirstOrDefaultAsync(u => u.UserName == userNameFromToken);
                    if (usertmp != null)
                    {
                        isAdult = usertmp.IsOfMajorityAge == true;
                    }
                }
            }
            if (!string.IsNullOrEmpty(UserSearch) && string.IsNullOrEmpty(nameCocktail))
            {
                string? userNameFromToken = null;
                if (User.Identity?.IsAuthenticated == true)
                    userNameFromToken = User.FindFirst(ClaimTypes.Name)?.Value?.ToLower();

                var userSearchLower = UserSearch.ToLower();
                var users = await _context.DbUser
                    .Where(u =>
                        u.UserName.ToLower().StartsWith(userSearchLower) &&
                        (userNameFromToken == null || u.UserName.ToLower() != userNameFromToken))
                    .Select(u => UtilsUserController.UserToDto(u))
                    .ToListAsync();
                if (users.Count == 0)
                {
                    users = await _context.DbUser
                        .Where(u =>
                            u.UserName.ToLower().Contains(userSearchLower) &&
                            (userNameFromToken == null || u.UserName.ToLower() != userNameFromToken))
                        .Select(u => UtilsUserController.UserToDto(u))
                        .ToListAsync();
                }
                return Ok(new
                {
                    TotalResult = users.Count,
                    TotalPages = 1,
                    CurrentPage = 1,
                    PageSize = 10,
                    Users = users
                });
            }

            if (!isAdult && string.IsNullOrEmpty(UserSearch) && string.IsNullOrEmpty(alcoholic))
            {
               query = query.Where(c => c.StrAlcoholic != null && c.StrAlcoholic.ToLower().Contains("Non alcoholic"));
            }
            // Verifica se ci sono filtri applicati
            bool noFilter = string.IsNullOrEmpty(nameCocktail) &&
                string.IsNullOrEmpty(glass) &&
                string.IsNullOrEmpty(ingredient) &&
                string.IsNullOrEmpty(category) &&
                string.IsNullOrEmpty(alcoholic);

        
            
            // Solo cocktail pubblici
            query = query.Where(c => c.PublicCocktail == true || c.PublicCocktail == null);
            
            if (!string.IsNullOrEmpty(glass))
                query = query.Where(c => c.StrGlass != null && c.StrGlass.ToLower().Contains(glass.ToLower()));

            if (!string.IsNullOrEmpty(category))
                query = query.Where(c => c.StrCategory != null && c.StrCategory.ToLower().Contains(category.ToLower()));

            if (!string.IsNullOrEmpty(alcoholic))
                query = query.Where(c => c.StrAlcoholic != null && c.StrAlcoholic.ToLower().Contains(alcoholic.ToLower()));

            if (!string.IsNullOrEmpty(nameCocktail))
                query = query.Where(c => c.StrDrink != null && c.StrDrink.ToLower().Contains(nameCocktail.ToLower()));
            if (!string.IsNullOrEmpty(ingredient))
            {
                var allCocktails = await query.ToListAsync();
                var ingredients = ingredient.ToLower();
                var IngredientStart = allCocktails
                    .Where(c => UtilsCocktail.GetIngredients(c)
                        .Any(i => i.ToLower().StartsWith(ingredients)))
                    .ToList();
                var IngredientContains = allCocktails
                    .Where(c => UtilsCocktail.GetIngredients(c)
                        .Any(i => i.ToLower().Contains(ingredients) && !i.ToLower().StartsWith(ingredients)))
                    .ToList();
                cocktailListIngredient = IngredientStart
                    .Concat(IngredientContains)
                    .GroupBy(c => c.Id)
                    .Select(g => g.First())
                    .ToList();
                pageSize = 50;
                totalItems = cocktailListIngredient.Count;
                totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
                var PageCocktails = cocktailListIngredient
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                var cocktailDtos = PageCocktails
                    .Select(c => UtilsCocktail.CocktailToDto(c))
                    .ToList();
                return Ok(new
                {
                    TotalResult = totalItems,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize,
                    Cocktails = cocktailDtos
                });
            }

            totalItems = await query.CountAsync();
            totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Ordinamento per nome, con precedenza se inizia con il testo cercato
            if (!string.IsNullOrEmpty(nameCocktail))
            {
                string name = nameCocktail.ToLower();
                query = query
                    .OrderBy(c => c.StrDrink == null || !c.StrDrink.ToLower().StartsWith(name))
                    .ThenBy(c => c.StrDrink);
            }
            else
            {
                query = query.OrderBy(c => c.StrDrink);
            }

            var cocktailList = await query.ToListAsync();

            var cocktailDto = cocktailList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => UtilsCocktail.CocktailToDto(c))
                .ToList();

            return Ok(new
            {
                TotalResult = totalItems,
                TotalPages = totalPages,
                CurrentPage = page < 1 ? 1 : page,
                PageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize),
                Cocktails = cocktailDto
            });
        }

        [Authorize]
        [HttpGet("MyCocktails")]
        public async Task<ActionResult<IEnumerable<CocktailDto>>> GetMyCocktails()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized("User not found.");
            }

            var user = await _context.DbUser.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
            {
                return Unauthorized("User not found or not accepted cookies.");
            }

            var cocktails = await _context.DbCocktails
                .Where(c => c.UserIdCocktail == user.Id)
                .ToListAsync();
            var cocktailDtos = cocktails
                .Select(c => UtilsCocktail.CocktailToDto(c))
                .ToList();
            
            return Ok(cocktailDtos);
        }

        [Authorize]
        [HttpPost("CocktailCreate")]
        public async Task<IActionResult> CreateCoctail([FromBody] CocktailCreate cocktailCreate)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized("User not found.");
            }

            var user = await _context.DbUser.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
            {
                return Unauthorized("User not found or not accepted cookies.");
            }
            if (string.IsNullOrEmpty(cocktailCreate.StrDrink))
            {
                return BadRequest("Cocktail name cannot be empty.");
            }
            var cocktail = await _context.DbCocktails
                .FirstOrDefaultAsync(c => c.StrDrink == cocktailCreate.StrDrink);

            if (cocktail != null)
            {
                return BadRequest("Cocktail already exists.");
            }

            var newcocktail = UtilsCocktail.CreateNewCocktail(cocktailCreate, user.Id);

            var ingredientList = UtilsCocktail.IngredientToList(newcocktail);
            bool haIngredientiAlcolici = UtilsCocktail.CocktailIsAlcoholic(ingredientList);

            var StrAlcoholic = haIngredientiAlcolici ? "Alcoholic" : "Non alcoholic";
            
            newcocktail.StrAlcoholic = StrAlcoholic;
            try
            {
                _context.DbCocktails.Add(newcocktail);
                await _context.SaveChangesAsync();
                
                return Ok(new { id = newcocktail.IdDrink, Message = "Cocktail creato con successo !!!", newcocktail });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating cocktail: {ex.Message}");
            }
        }


        [Authorize]
        [HttpGet("IngredientSearch/SearchIngredient")]
        public async Task<IActionResult> GetIngredientSearch(
            [FromQuery] int id,
            [FromQuery] string ingredient = "",
            [FromQuery] int max = 10
        ){
            if (string.IsNullOrEmpty(ingredient))
                return BadRequest("Ingredient cannot be empty.");
            var usernamebytoken = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(usernamebytoken))
                return Unauthorized("User not found.");
            var user = await _context.DbUser.FirstAsync(u => u.Id == id);
            if (user == null)
                return NotFound("User not found.");

            var isAdult = user.IsOfMajorityAge == true;
            var listIngredient = isAdult ? UtilsCocktail.SearchIngredients(ingredient, _context ,max) : UtilsCocktail.SearchNonAlcoholicIngredients(ingredient, _context ,max);
            
            return Ok(new
            {
                ingredients = listIngredient
            });
        }

        [Authorize]
        [HttpGet("SearchMeasureType/searchMeasure")]
        public async Task<IActionResult> GetMeasureTypeSearch(
            [FromQuery] int id, 
            [FromQuery] string measure = "",
            [FromQuery] int max = 40
        ){
            if (string.IsNullOrEmpty(measure))
                return BadRequest("Measure cannot be empty.");

            var usernamebytoken = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(usernamebytoken))
                return Unauthorized("User not found.");

            var user = await _context.DbUser.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return NotFound("User not found.");

            var listMeasure = UtilsCocktail.SearchMeasureType(measure, max, _context);
            
            return Ok(new
            {
                measureTypes = listMeasure
            });
        }

        [Authorize]
        [HttpGet("SearchGlass/searchGlass")]
        public async Task<IActionResult> GetGlassSearch(
            [FromQuery] int id, 
            [FromQuery] string glass = "",
            [FromQuery] int max = 40
        ){
            if (string.IsNullOrEmpty(glass))
                return BadRequest("Glass cannot be empty.");

            var usernamebytoken = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(usernamebytoken))
                return Unauthorized("User not found.");

            var user = await _context.DbUser.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return NotFound("User not found.");

            var listGlass = UtilsCocktail.SearchGlassType(glass, _context ,max);
            
            return Ok(new{ glassTypes = listGlass });
        }

        [Authorize]
        [HttpGet("SearchCategory/searchCategory")]
        public async Task<IActionResult> GetCategorySearch(
            [FromQuery] int id, 
            [FromQuery] string category = "",
            [FromQuery] int max = 40
        ){
            if (string.IsNullOrEmpty(category))
                return BadRequest("Category cannot be empty.");

            var usernamebytoken = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(usernamebytoken))
                return Unauthorized("User not found.");

            var user = await _context.DbUser.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return NotFound("User not found.");

            var listCategory = UtilsCocktail.SearchCategoryType(category, max, _context);
            
            return Ok(new { categoryTypes = listCategory });
        }

        [Authorize]
        [HttpPut("CocktailUpdate/{idDrink}")]
        public async Task<IActionResult> UpdateCocktail(int idDrink, [FromBody] CocktailCreate updatedCocktail)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
                return Unauthorized("User not authenticated.");

            var user = await _context.DbUser.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
                return NotFound("User not found or does not accept cookies.");

            var cocktail = await _context.DbCocktails
                .FirstOrDefaultAsync(c => c.Id == idDrink && c.UserIdCocktail == user.Id);

            if (cocktail == null)
                return NotFound("Cocktail not found or does not belong to you.");

            UtilsCocktail.UpdateCocktail(cocktail, updatedCocktail);
            try{
                await _context.SaveChangesAsync();
                return Ok(new { Message = "Cocktail aggiornato con successo!" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating cocktail: {ex.Message}");
            }
        }

        [AllowAnonymous]
        [HttpGet("GetCocktailCreatorByUser/{id}")]
        public async Task<IActionResult> GetCocktailCreatorByUser(int id)
        {
            var user = await _context.DbUser.FirstAsync(u => u.Id == id);

            if (user == null)
                return NotFound("User not found.");

            var cocktialUser = _context.DbCocktails
                .Where(c => c.PublicCocktail == true && c.UserIdCocktail == user.Id)
                .ToList();
            var cocktailDto = cocktialUser.Select(c => UtilsCocktail.CocktailToDto(c)).ToList();
            return Ok(cocktailDto);
        }

        // Cocktail Delete (User)
        [Authorize]
        [HttpDelete("CocktailDelete/{idDrink}")]
        public async Task<IActionResult> DeleteCocktail(int idDrink)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
                return Unauthorized("User not authenticated.");

            var user = await _context.DbUser.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
                return NotFound("User not found or does not accept cookies.");

            var cocktail = await _context.DbCocktails
                .FirstOrDefaultAsync(c => c.Id == idDrink && c.UserIdCocktail == user.Id);

            if (cocktail == null)
                return NotFound("Cocktail not found or does not belong to you.");
            try{
                _context.DbCocktails.Remove(cocktail);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Cocktail eliminato con successo!" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting cocktail: {ex.Message}");
            }
        }

        // getione delle immagini dei cocktail creati o modificati dall'utente
        [Authorize]
        [HttpPost("{id}/UploadImageCocktail-local")]
        public async Task<IActionResult> UploadImageCocktailLocal(int id, IFormFile file)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
                return Unauthorized("User not authenticated.");

            var user = await _context.DbUser.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null || user?.Id != id) 
                return NotFound("User not found ");


            if (file == null || file.Length == 0)
                return BadRequest("File not provided.");

            var newPublicId = $"cocktail_images/{username}_{id}_{DateTime.UtcNow.Ticks}"; // Genera un nuovo publicId unico
            var imageUrl = await _cloudinaryService.UploadImageAsync(file, newPublicId);
            if (string.IsNullOrEmpty(imageUrl))
                return BadRequest("Error uploading image.");
            
            return Ok(new{ ImageUrl = imageUrl});

        }

        [Authorize]
        [HttpPost("{id}/UploadImageCocktail-url")]
        public async Task<IActionResult> UploadImageCocktailUrl(int id, [FromBody] string imageUrl)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
                return Unauthorized("User not authenticated.");
            var user = await _context.DbUser.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
                return NotFound("User not found or does not accept cookies.");

            var cocktail = await _context.DbCocktails
                .FirstOrDefaultAsync(c => c.Id == id && c.UserIdCocktail == user.Id);

            if (cocktail == null)
                return NotFound("Cocktail not found or does not belong to you.");

            if (string.IsNullOrEmpty(imageUrl))
                return BadRequest("Image URL not provided.");

            if(!string.IsNullOrEmpty(cocktail.StrDrinkThumb))
            {
                try{
                    var uri = new Uri(cocktail.StrDrinkThumb);
                   var segments = uri.AbsolutePath.Split('/');
                    var folder = string.Join("/", segments.Skip(Array.IndexOf(segments, "upload") + 1));
                    var publicId = Path.Combine(Path.GetDirectoryName(folder) ?? "", Path.GetFileNameWithoutExtension(folder)).Replace("\\", "/");

                    await _cloudinaryService.DeleteImageAsync(publicId);
                }
                catch (Exception ex)
                {
                    return BadRequest($"Error deleting old image: {ex.Message}");
                }
            }

            var newPublicId = $"cocktail_images/{username}_{id}_{DateTime.UtcNow.Ticks}"; // Genera un nuovo publicId unico
            var imageUrlCloudinary = await _cloudinaryService.UploadImageAsyncUrl(imageUrl, newPublicId);
            if (string.IsNullOrEmpty(imageUrlCloudinary))
                return BadRequest("Error uploading image.");
            
            cocktail.StrDrinkThumb = imageUrlCloudinary;
            await _context.SaveChangesAsync();
            return Ok(new
            {
                Message = "Image uploaded successfully!",
                ImageUrl = cocktail.StrDrinkThumb,
                CocktailId = cocktail.Id,
                cocktail.StrDrink
            });
        }


        [Authorize]
        [HttpPost("like/{Id}")]
        public async Task<IActionResult> CocktailLicke(int Id){
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
                return Unauthorized("User not authenticated.");

            var cocktail = await _context.DbCocktails.Include(c => c.UserLikes).FirstOrDefaultAsync(c => c.Id == Id);
            if (cocktail == null)
                return NotFound("Cocktail not found.");

            var user = await _context.DbUser.Include(u=> u.CocktailsLike).FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
                return NotFound("User not found.");

            if (user.CocktailsLike.Any(c => c.Id == Id))
            {
                user.CocktailsLike.Remove(cocktail); 
                cocktail.UserLikes.Remove(user);
                cocktail.Likes = Math.Max(0, cocktail.Likes - 1);
            }
            else
            {
                user.CocktailsLike.Add(cocktail);
                cocktail.UserLikes.Add(user);
                cocktail.Likes += 1;
            }
            
            await _context.SaveChangesAsync();
            return Ok(new { Message = $"Cocktail like status updated successfully! NameCocktail {cocktail.StrDrink} = {cocktail.Likes} "});
        }


        [HttpGet("GetUserCocktailLikes")]
        public async Task<IActionResult> GetUserCocktailLikes(int id)
        {
            var cocktail = await _context.DbCocktails
                .Include(c => c.UserLikes)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cocktail == null)
                return NotFound("Cocktail not found.");
            
            var users = cocktail.UserLikes
                .Select(u => UtilsUserController.UserToDto(u)).ToList();
           
            if (users == null || !users.Any())
                return NotFound("No users liked this cocktail.");
            return Ok(users);
        }

        [HttpGet("GetCountCocktailLikes/{id}")]
        public async Task<IActionResult> GetCountCocktailLikes(int id)
        {
            var cocktail = await _context.DbCocktails.FirstOrDefaultAsync(c => c.Id == id);
            if (cocktail == null)
                return NotFound("Cocktail not found.");

            return Ok( cocktail.Likes );
        }

        [HttpGet("ingredients")]
        public async Task<IActionResult> GetUniqueIngredients()
        {
            var cocktails = await _context.DbCocktails.ToListAsync(); // carica in memoria

            var ingredienti = cocktails
                .SelectMany(c =>
                    Enumerable.Range(1, 15)
                        .Select(i => (string?)typeof(Cocktail).GetProperty($"StrIngredient{i}")?.GetValue(c)))
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .Select(i => i!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(i => i)
                .ToList();

            return Ok(ingredienti);
        }


        [Authorize]
        [HttpGet("SearchUser/{username}")]
        public async Task<IActionResult> SearchUser(string username)
        {
            var userNameFromToken = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userNameFromToken))
                return Unauthorized("User not authenticated.");
            var userFromToken = await _context.DbUser
                .FirstOrDefaultAsync(u => u.UserName == userNameFromToken);
            if (userFromToken == null)
                return NotFound("User not found.");

            if (string.IsNullOrEmpty(username))
                return BadRequest("Username cannot be empty.");
            
            // 
            var users = await _context.DbUser
            .Where(u => u.UserName.ToLower().StartsWith(username.ToLower()) && u.UserName != userNameFromToken)
            .Select(u => UtilsUserController.UserToDto(u)).ToListAsync();

            if (users == null || users.Count == 0)
            {
                users = await _context.DbUser
                    .Where(u => u.UserName.ToLower().Contains(username.ToLower())
                                && u.UserName != userNameFromToken)
                    .Select(u => UtilsUserController.UserToDto(u)).ToListAsync();
            }
            return Ok(users);
        }
    }
}
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
                .Where(c => c.Id == id && c.PublicCocktail == true)
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
                    if (user != null)
                    {
                        bool exists = await _context.DbUserHistorySearch
                            .AnyAsync(h => h.UserName == user.UserName 
                            && cocktail != null && h.SearchText == cocktail.StrDrink);
                            if (!exists && !string.IsNullOrEmpty(cocktail?.StrDrink) && user.AcceptCookies == true)
                            {
                                _context.DbUserHistorySearch.Add(new UserHistorySearch
                                {
                                    UserName = user.UserName,
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
            [FromQuery] string description = "",
            [FromQuery] string completeSearch = "",
            [FromQuery] string cocktailLicke = "",
            [FromQuery] string UsernameCreateCocktail = "",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
        {
            IQueryable<Cocktail> query = _context.DbCocktails.AsQueryable();

            var isAdult = true;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userNameFromToken = User.FindFirst(ClaimTypes.Name)?.Value;
                if (!string.IsNullOrEmpty(userNameFromToken))
                {
                    var usertmp = await _context.DbUser
                        .FirstOrDefaultAsync(u => u.UserName == userNameFromToken && u.AcceptCookies == true);
                    if (usertmp != null)
                    {
                        isAdult = usertmp.IsOfMajorityAge == true;
                    }
                }
            }

            if (!isAdult)
            {
                query = query.Where(c => c.StrAlcoholic != null && c.StrAlcoholic.Equals("Non alcoholic", StringComparison.OrdinalIgnoreCase));
            }
            // Verifica se ci sono filtri applicati
            bool noFilter = string.IsNullOrEmpty(nameCocktail) &&
                string.IsNullOrEmpty(glass) &&
                string.IsNullOrEmpty(ingredient) &&
                string.IsNullOrEmpty(category) &&
                string.IsNullOrEmpty(alcoholic) &&
                string.IsNullOrEmpty(description) &&
                string.IsNullOrEmpty(UsernameCreateCocktail);
            
            if(!string.IsNullOrEmpty(UserSearch) && noFilter == true && nameCocktail == "")
            {
                string? userNameFromToken = null;
                if (User.Identity?.IsAuthenticated == true)
                    userNameFromToken = User.FindFirst(ClaimTypes.Name)?.Value;

                var users = await _context.DbUser
                    .Where(u => u.UserName.ToLower().StartsWith(UserSearch.ToLower())
                                && u.AcceptCookies == true
                                && (userNameFromToken == null || u.UserName != userNameFromToken))
                    .Select(u => UtilsUserController.UserToDto(u)).ToListAsync();

                // Se non trova niente, cerca chi contiene la stringa
                if (users == null || users.Count == 0)
                {
                    users = await _context.DbUser
                        .Where(u => u.UserName.ToLower().Contains(UserSearch.ToLower())
                                    && u.AcceptCookies == true
                                    && (userNameFromToken == null || u.UserName != userNameFromToken))
                        .Select(u => UtilsUserController.UserToDto(u)).ToListAsync();
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
            
            // Solo cocktail pubblici
            query = query.Where(c => c.PublicCocktail == true || c.PublicCocktail == null);
            
            if (!string.IsNullOrEmpty(glass))
                query = query.Where(c => c.StrGlass != null && c.StrGlass.ToLower().Contains(glass.ToLower()));

            if (!string.IsNullOrEmpty(category))
                query = query.Where(c => c.StrCategory != null && c.StrCategory.ToLower().Contains(category.ToLower()));

            if (!string.IsNullOrEmpty(alcoholic))
                query = query.Where(c => c.StrAlcoholic != null && c.StrAlcoholic.ToLower().Contains(alcoholic.ToLower()));

            if (!string.IsNullOrEmpty(UsernameCreateCocktail))
                query = query.Where(c => c.UserNameCocktail != null && c.UserNameCocktail.ToLower().Contains(UsernameCreateCocktail.ToLower()));

            if (!string.IsNullOrEmpty(nameCocktail))
                query = query.Where(c => c.StrDrink != null && c.StrDrink.ToLower().Contains(nameCocktail.ToLower()));
            else if (!string.IsNullOrEmpty(ingredient))
                query = query.Where(
                    c => (c.StrIngredient1 != null && c.StrIngredient1.ToLower().Contains(ingredient.ToLower())) ||
                        (c.StrIngredient2 != null && c.StrIngredient2.ToLower().Contains(ingredient.ToLower())) ||
                        (c.StrIngredient3 != null && c.StrIngredient3.ToLower().Contains(ingredient.ToLower())) ||
                        (c.StrIngredient4 != null && c.StrIngredient4.ToLower().Contains(ingredient.ToLower())) ||
                        (c.StrIngredient5 != null && c.StrIngredient5.ToLower().Contains(ingredient.ToLower())) ||
                        (c.StrIngredient6 != null && c.StrIngredient6.ToLower().Contains(ingredient.ToLower())) ||
                        (c.StrIngredient7 != null && c.StrIngredient7.ToLower().Contains(ingredient.ToLower())) ||
                        (c.StrIngredient8 != null && c.StrIngredient8.ToLower().Contains(ingredient.ToLower())) ||
                        (c.StrIngredient9 != null && c.StrIngredient9.ToLower().Contains(ingredient.ToLower())) ||
                        (c.StrIngredient10 != null && c.StrIngredient10.ToLower().Contains(ingredient.ToLower())) ||
                        (c.StrIngredient11 != null && c.StrIngredient11.ToLower().Contains(ingredient.ToLower())) ||
                        (c.StrIngredient12 != null && c.StrIngredient12.ToLower().Contains(ingredient.ToLower())) ||
                        (c.StrIngredient13 != null && c.StrIngredient13.ToLower().Contains(ingredient.ToLower())) ||
                        (c.StrIngredient14 != null && c.StrIngredient14.ToLower().Contains(ingredient.ToLower())) ||
                        (c.StrIngredient15 != null && c.StrIngredient15.ToLower().Contains(ingredient.ToLower()))
                );
            if (!string.IsNullOrEmpty(description))
                query = query.Where(c => c.StrInstructions != null && c.StrInstructions.ToLower().Contains(description.ToLower()));

            User? user = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userNameFromToken = User.FindFirst(ClaimTypes.Name)?.Value;
                user = await _context.DbUser
                    .Include(u => u.CocktailsLike)
                    .FirstOrDefaultAsync(u => u.UserName == userNameFromToken && u.AcceptCookies == true);

                // Salva lo storico delle ricerche solo se è una ricerca completa
                if (user != null && !string.IsNullOrEmpty(completeSearch))
                {
                    bool alreadyInHistory = await _context.DbUserHistorySearch
                        .AnyAsync(h => h.UserName == user.UserName && h.SearchText == completeSearch);
                    if (!alreadyInHistory && user.AcceptCookies == true)
                    {
                        _context.DbUserHistorySearch.Add(new UserHistorySearch
                        {
                            UserName = user.UserName,
                            SearchText = completeSearch,
                            SearchDate = DateTime.UtcNow
                        });
                        await _context.SaveChangesAsync();
                    }
                }

                // Se si richiedono solo i like, override sulla query (niente consigli, solo quelli likati)
                if (!string.IsNullOrEmpty(cocktailLicke) && cocktailLicke.ToLower() == "true")
                {
                    var likedCocktailIds = user?.CocktailsLike.Select(c => c.Id).ToList();
                    
                    if (likedCocktailIds != null && likedCocktailIds.Any())
                        query = query.Where(c => likedCocktailIds.Contains(c.Id));
                    else
                        query = query.Where(c => c.Id == 0);
                }
            }

            // Calcolo risultati dopo i filtri
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

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

            // Calcolo lo score per ogni cocktail (solo se autenticato)
            var searchHistory = user != null
                ? await _context.DbUserHistorySearch
                    .Where(s => s.UserName == user.UserName)
                    .OrderByDescending(s => s.SearchDate)
                    .Take(10)
                    .Select(s => s.SearchText!.ToLower())
                    .ToListAsync()
                : new List<string>();
            var likedList = user != null ? user.CocktailsLike.ToList() : new List<Cocktail>();

            var cocktailScores = cocktailList
                .Select(c => new {
                    Cocktail = c,
                    Score = user != null ? UtilsCocktail.GetSuggestionScore(c, user, searchHistory, likedList) : 0
                })
                // SE sei autenticato e non hai filtri, mostra solo i consigliati (score > 0)
                // SE ci sono filtri, mostra tutto ma ordinato per score
                .Where(c => !noFilter || c.Score > 0)
                .OrderByDescending(c => c.Score)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => UtilsCocktail.CocktailToDto(c.Cocktail)).ToList();

            return Ok(new
            {
                TotalResult = totalItems,
                TotalPages = totalPages,
                CurrentPage = page < 1 ? 1 : page,
                PageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize),
                Cocktails = cocktailScores
            });
        }


        // Cocktail Create o Modificati (User)

        [Authorize]
        [HttpGet("MyCocktails")]
        public async Task<ActionResult<IEnumerable<CocktailDto>>> GetMyCocktails()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized("User not found.");
            }

            var cocktails = await _context.DbCocktails
                .Where(c => c.UserNameCocktail == username)
                .ToListAsync();
            return Ok(cocktails);
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

            var user = await _context.DbUser.FirstOrDefaultAsync(u => u.UserName == username && u.AcceptCookies == true);
            if (user == null)
            {
                return Unauthorized("User not found or not accepted cookies.");
            }

            var cocktail = await _context.DbCocktails
                .FirstOrDefaultAsync(c => c.StrDrink == cocktailCreate.StrDrink);

            if (cocktail != null)
            {
                return BadRequest("Cocktail already exists.");
            }

            var newcocktail = UtilsCocktail.CreateNewCocktail(cocktailCreate, username);

            var ingredientList = UtilsCocktail.IngredientToList(newcocktail);
            bool haIngredientiAlcolici = UtilsCocktail.CocktailIsAlcoholic(ingredientList);

            if (!user.IsOfMajorityAge.GetValueOrDefault(true))
            {
                // Solo analcolici permessi per i minorenni
                if (cocktailCreate.StrAlcoholic != "Non alcoholic")
                    return BadRequest("Se sei minorenne puoi creare solo cocktail analcolici!");

                if (haIngredientiAlcolici)
                    return BadRequest("Se sei minorenne non puoi inserire ingredienti alcolici!");
            }

            // Validazione della coerenza tra ingredienti e misure
            var validationError = UtilsCocktail.ValidateIngredientMeasureConsistency(newcocktail);
            if (validationError != null)
            {
                return BadRequest(validationError);
            }

            // Validazione della classe di volume del cocktail
            var volumeError = UtilsCocktail.ValidateVolumeClassCocktail(newcocktail, UtilsCocktail.GlassCapacity);
            if (volumeError != null)
            {
                return BadRequest(volumeError);
            }
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
        [HttpGet("IngedientSearch")]
        public async Task<IActionResult> GetIngredientSearch(
            [FromQuery] string UserName = "",
            [FromQuery] string ingredient = "",
            [FromQuery] int max = 10
        ){
            if (string.IsNullOrEmpty(ingredient))
                return BadRequest("Ingredient cannot be empty.");
            var usernamebytoken = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(usernamebytoken))
                return Unauthorized("User not found.");

            if (string.IsNullOrEmpty(UserName) || UserName != usernamebytoken)
                return BadRequest("UserName not match.");
            var user = await _context.DbUser
                .FirstOrDefaultAsync(u => u.UserName == UserName && u.AcceptCookies == true);
            if (user == null)
                return NotFound("User not found.");
            

            var isAdult = user.IsOfMajorityAge == true;
            var listIngredient = isAdult
            ? UtilsCocktail.SearchIngredients(ingredient, max)
            : UtilsCocktail.SearchNonAlcoholicIngredients(ingredient, max);
            
            return Ok(new
            {
                isAdult,
                ingredients = listIngredient
            });
        }

        [Authorize]
        [HttpGet("SearchMeasureType")]
        public async Task<IActionResult> GetMeasureTypeSearch(
            [FromQuery] string UserName = "", 
            [FromQuery] string measure = "",
            [FromQuery] int max = 10
        ){
            if (string.IsNullOrEmpty(measure))
                return BadRequest("Measure cannot be empty.");

            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
                return Unauthorized("User not found.");

            var user = await _context.DbUser
                .FirstOrDefaultAsync(u => u.UserName == UserName && u.AcceptCookies == true);
            if (user == null)
                return NotFound("User not found.");

            var listMeasure = UtilsCocktail.SearchMeasureType(measure, max);
            
            return Ok(new
            {
                measureTypes = listMeasure
            });
        }

        // Cocktail Update (User)
        [Authorize]
        [HttpPut("CocktailUpdate/{idDrink}")]
        public async Task<IActionResult> UpdateCocktail(int idDrink, [FromBody] CocktailCreate updatedCocktail)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
                return Unauthorized("User not authenticated.");

            var cocktail = await _context.DbCocktails
                .FirstOrDefaultAsync(c => c.Id == idDrink && c.UserNameCocktail == username);

            if (cocktail == null)
                return NotFound("Cocktail not found or does not belong to you.");

           UtilsCocktail.UpdateCocktail(cocktail, updatedCocktail);

            // Validazione della coerenza tra ingredienti e misure
            var validationError = UtilsCocktail.ValidateIngredientMeasureConsistency(cocktail);
            if (validationError != null)
            {
                return BadRequest(validationError);
            }
            // Validazione della classe di volume del cocktail
            var volumeError = UtilsCocktail.ValidateVolumeClassCocktail(cocktail, UtilsCocktail.GlassCapacity);
            if (volumeError != null)
            {
                return BadRequest(volumeError);
            }
            try{
                await _context.SaveChangesAsync();
                return Ok(new { Message = "Cocktail aggiornato con successo!" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating cocktail: {ex.Message}");
            }
        }


        // Cocktail Delete (User)
        [Authorize]
        [HttpDelete("CocktailDelete/{idDrink}")]
        public async Task<IActionResult> DeleteCocktail(int idDrink)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
                return Unauthorized("User not authenticated.");

            var cocktail = await _context.DbCocktails
                .FirstOrDefaultAsync(c => c.Id == idDrink && c.UserNameCocktail == username);

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

            var cocktail = await _context.DbCocktails
                .FirstOrDefaultAsync(c => c.Id == id && c.UserNameCocktail == username);

            if (cocktail == null)
                return NotFound("Cocktail not found or does not belong to you.");

            if (file == null || file.Length == 0)
                return BadRequest("File not provided.");

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
            var imageUrl = await _cloudinaryService.UploadImageAsync(file, newPublicId);
            if (string.IsNullOrEmpty(imageUrl))
                return BadRequest("Error uploading image.");
            
            cocktail.StrDrinkThumb = imageUrl;
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
        [HttpPost("{id}/UploadImageCocktail-url")]
        public async Task<IActionResult> UploadImageCocktailUrl(int id, [FromBody] string imageUrl)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
                return Unauthorized("User not authenticated.");

            var cocktail = await _context.DbCocktails
                .FirstOrDefaultAsync(c => c.Id == id && c.UserNameCocktail == username);

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

            var cocktail = await _context.DbCocktails.Include(c => c.UserLikes).FirstOrDefaultAsync(c => c.Id == Id && c.PublicCocktail == true);
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
                .FirstOrDefaultAsync(u => u.UserName == userNameFromToken && u.AcceptCookies == true);
            if (userFromToken == null)
                return NotFound("User not found.");

            if (string.IsNullOrEmpty(username))
                return BadRequest("Username cannot be empty.");
            
            // 
            var users = await _context.DbUser
            .Where(u => u.UserName.ToLower().StartsWith(username.ToLower())
                        && u.AcceptCookies == true
                        && u.UserName != userNameFromToken)
            .Select(u => UtilsUserController.UserToDto(u)).ToListAsync();

            if (users == null || users.Count == 0)
            {
                users = await _context.DbUser
                    .Where(u => u.UserName.ToLower().Contains(username.ToLower())
                                && u.AcceptCookies == true
                                && u.UserName != userNameFromToken)
                    .Select(u => UtilsUserController.UserToDto(u)).ToListAsync();
            }
            return Ok(users);
        }
    }
}
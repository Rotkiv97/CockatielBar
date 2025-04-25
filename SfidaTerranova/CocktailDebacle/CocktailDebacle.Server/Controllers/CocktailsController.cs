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
using CocktailDebacle.Server.DTOs; // Importa il namespace del DTO

using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace CocktailDebacle.Server.Controllers
{
    [Route("api/[controller]")]
    public class CocktailsController : ControllerBase
    {
       
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;

        private readonly Dictionary<string, int> _glassCapacity = new Dictionary<string, int>
        {
            { "Highball glass", 350 },
            { "Cocktail glass", 150 },
            { "Old-fashioned glass", 300 },
            { "Collins glass", 400 },
            { "Margarita glass", 300 },
            { "Pint glass", 500 },
            { "Shot glass", 50 },
            { "Whiskey sour glass", 250 },
            { "Hurricane glass", 400 },
            { "Champagne flute", 200 },
            { "Beer mug", 500 },
            { "Brandy snifter", 300 },
            { "Cordial glass", 100 },
            { "Copper mug", 350 },
            { "Irish coffee cup", 300 }
        };

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
                .Select(c => new CocktailDto
                {
                    IdDrink = c.IdDrink ?? string.Empty,
                    StrDrink = c.StrDrink ?? string.Empty,
                    StrCategory = c.StrCategory ?? string.Empty,
                    StrAlcoholic = c.StrAlcoholic ?? string.Empty,
                    StrGlass = c.StrGlass ?? string.Empty,
                    StrInstructions = c.StrInstructions ?? string.Empty,
                    StrDrinkThumb = c.StrDrinkThumb ?? string.Empty,
                    Ingredients = new List<string>
                    {
                        c.StrIngredient1 ?? string.Empty, c.StrIngredient2 ?? string.Empty, c.StrIngredient3 ?? string.Empty, c.StrIngredient4 ?? string.Empty, c.StrIngredient5 ?? string.Empty,
                        c.StrIngredient6 ?? string.Empty, c.StrIngredient7 ?? string.Empty, c.StrIngredient8 ?? string.Empty, c.StrIngredient9 ?? string.Empty, c.StrIngredient10 ?? string.Empty,
                        c.StrIngredient11 ?? string.Empty, c.StrIngredient12 ?? string.Empty, c.StrIngredient13 ?? string.Empty, c.StrIngredient14 ?? string.Empty, c.StrIngredient15 ?? string.Empty
                    },
                    Measures = new List<string>
                    {
                        c.StrMeasure1 ?? string.Empty, c.StrMeasure2 ?? string.Empty, c.StrMeasure3 ?? string.Empty, c.StrMeasure4 ?? string.Empty, c.StrMeasure5 ?? string.Empty,
                        c.StrMeasure6 ?? string.Empty, c.StrMeasure7 ?? string.Empty, c.StrMeasure8 ?? string.Empty, c.StrMeasure9 ?? string.Empty, c.StrMeasure10 ?? string.Empty,
                        c.StrMeasure11 ?? string.Empty, c.StrMeasure12 ?? string.Empty, c.StrMeasure13 ?? string.Empty, c.StrMeasure14 ?? string.Empty, c.StrMeasure15 ?? string.Empty
                    },
                })
                .ToListAsync();
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

            var cocktail = new CocktailDto
            {
                Id = cocktailEntity.Id,
                IdDrink = cocktailEntity.IdDrink ?? string.Empty,
                StrDrink = cocktailEntity.StrDrink ?? string.Empty,
                StrCategory = cocktailEntity.StrCategory ?? string.Empty,
                StrAlcoholic = cocktailEntity.StrAlcoholic ?? string.Empty,
                StrGlass = cocktailEntity.StrGlass ?? string.Empty,
                StrInstructions = cocktailEntity.StrInstructions ?? string.Empty,
                StrDrinkThumb = cocktailEntity.StrDrinkThumb ?? string.Empty,
                Ingredients = new List<string>
                {
                    cocktailEntity.StrIngredient1, cocktailEntity.StrIngredient2, cocktailEntity.StrIngredient3,
                    cocktailEntity.StrIngredient4, cocktailEntity.StrIngredient5, cocktailEntity.StrIngredient6,
                    cocktailEntity.StrIngredient7, cocktailEntity.StrIngredient8, cocktailEntity.StrIngredient9,
                    cocktailEntity.StrIngredient10, cocktailEntity.StrIngredient11, cocktailEntity.StrIngredient12,
                    cocktailEntity.StrIngredient13, cocktailEntity.StrIngredient14, cocktailEntity.StrIngredient15
                }.Where(i => !string.IsNullOrWhiteSpace(i)).ToList(),
                Measures = new List<string>
                {
                    cocktailEntity.StrMeasure1, cocktailEntity.StrMeasure2, cocktailEntity.StrMeasure3,
                    cocktailEntity.StrMeasure4, cocktailEntity.StrMeasure5, cocktailEntity.StrMeasure6,
                    cocktailEntity.StrMeasure7, cocktailEntity.StrMeasure8, cocktailEntity.StrMeasure9,
                    cocktailEntity.StrMeasure10, cocktailEntity.StrMeasure11, cocktailEntity.StrMeasure12,
                    cocktailEntity.StrMeasure13, cocktailEntity.StrMeasure14, cocktailEntity.StrMeasure15
                }.Where(m => !string.IsNullOrWhiteSpace(m)).ToList()
            };
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
                            if (!exists && !string.IsNullOrEmpty(cocktail?.StrDrink))
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
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        UserName = u.UserName,
                        Name = u.Name,
                        LastName = u.LastName,
                        Email = u.Email,
                        ImgProfileUrl = u.ImgProfileUrl
                    })
                    .ToListAsync();

                // Se non trova niente, cerca chi contiene la stringa
                if (users == null || users.Count == 0)
                {
                    users = await _context.DbUser
                        .Where(u => u.UserName.ToLower().Contains(UserSearch.ToLower())
                                    && u.AcceptCookies == true
                                    && (userNameFromToken == null || u.UserName != userNameFromToken))
                        .Select(u => new UserDto
                        {
                            Id = u.Id,
                            UserName = u.UserName,
                            Name = u.Name,
                            LastName = u.LastName,
                            Email = u.Email,
                            ImgProfileUrl = u.ImgProfileUrl
                        })
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
                    if (!alreadyInHistory)
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
                    Score = user != null ? GetSuggestionScore(c, user, searchHistory, likedList) : 0
                })
                // SE sei autenticato e non hai filtri, mostra solo i consigliati (score > 0)
                // SE ci sono filtri, mostra tutto ma ordinato per score
                .Where(c => !noFilter || c.Score > 0)
                .OrderByDescending(c => c.Score)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CocktailDto
                {
                    Id = c.Cocktail.Id,
                    IdDrink = c.Cocktail.IdDrink ?? string.Empty,
                    StrDrink = c.Cocktail.StrDrink ?? string.Empty,
                    StrCategory = c.Cocktail.StrCategory ?? string.Empty,
                    StrAlcoholic = c.Cocktail.StrAlcoholic ?? string.Empty,
                    StrGlass = c.Cocktail.StrGlass ?? string.Empty,
                    StrInstructions = c.Cocktail.StrInstructions ?? string.Empty,
                    StrDrinkThumb = c.Cocktail.StrDrinkThumb ?? string.Empty,
                    Ingredients = new List<string>
                    {
                        c.Cocktail.StrIngredient1 ?? string.Empty, c.Cocktail.StrIngredient2 ?? string.Empty, c.Cocktail.StrIngredient3 ?? string.Empty, c.Cocktail.StrIngredient4 ?? string.Empty, c.Cocktail.StrIngredient5 ?? string.Empty,
                        c.Cocktail.StrIngredient6 ?? string.Empty, c.Cocktail.StrIngredient7 ?? string.Empty, c.Cocktail.StrIngredient8 ?? string.Empty, c.Cocktail.StrIngredient9 ?? string.Empty, c.Cocktail.StrIngredient10 ?? string.Empty,
                        c.Cocktail.StrIngredient11 ?? string.Empty, c.Cocktail.StrIngredient12 ?? string.Empty, c.Cocktail.StrIngredient13 ?? string.Empty, c.Cocktail.StrIngredient14 ?? string.Empty, c.Cocktail.StrIngredient15 ?? string.Empty
                    }.Where(i => !string.IsNullOrWhiteSpace(i)).ToList(),
                    Measures = new List<string>
                    {
                        c.Cocktail.StrMeasure1 ?? string.Empty, c.Cocktail.StrMeasure2 ?? string.Empty, c.Cocktail.StrMeasure3 ?? string.Empty, c.Cocktail.StrMeasure4 ?? string.Empty, c.Cocktail.StrMeasure5 ?? string.Empty,
                        c.Cocktail.StrMeasure6 ?? string.Empty, c.Cocktail.StrMeasure7 ?? string.Empty, c.Cocktail.StrMeasure8 ?? string.Empty, c.Cocktail.StrMeasure9 ?? string.Empty, c.Cocktail.StrMeasure10 ?? string.Empty,
                        c.Cocktail.StrMeasure11 ?? string.Empty, c.Cocktail.StrMeasure12 ?? string.Empty, c.Cocktail.StrMeasure13 ?? string.Empty, c.Cocktail.StrMeasure14 ?? string.Empty, c.Cocktail.StrMeasure15 ?? string.Empty
                    }.Where(m => !string.IsNullOrWhiteSpace(m)).ToList(),
                    StrTags = c.Cocktail.StrTags ?? string.Empty
                })
                .ToList();

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
        public async Task<IActionResult> CreateCoctail([FromBody] CocktailCreate cocktailCreate){
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized("User not found.");
            }

            var cocktail = await _context.DbCocktails
                .FirstOrDefaultAsync(c => c.StrDrink == cocktailCreate.StrDrink);

            if (cocktail != null)
            {
                return BadRequest("Cocktail already exists.");
            }
            
            var newcocktail = new Cocktail
            {
                UserNameCocktail = username,
                PublicCocktail = cocktailCreate.PublicCocktail,
                dateCreated = DateTime.Now,
                Likes = 0,
                IdDrink = cocktailCreate.IdDrink,
                StrDrink = cocktailCreate.StrDrink,
                StrDrinkAlternate = cocktailCreate.StrDrinkAlternate,
                StrTags = cocktailCreate.StrTags,
                StrVideo = cocktailCreate.StrVideo,
                StrCategory = cocktailCreate.StrCategory,
                StrIBA = cocktailCreate.StrIBA,
                StrAlcoholic = cocktailCreate.StrAlcoholic,
                StrGlass = cocktailCreate.StrGlass,
                StrInstructions = cocktailCreate.StrInstructions,
                StrInstructionsES = cocktailCreate.StrInstructionsES,
                StrInstructionsDE = cocktailCreate.StrInstructionsDE,
                StrInstructionsFR = cocktailCreate.StrInstructionsFR,
                StrInstructionsIT = cocktailCreate.StrInstructionsIT,
                StrInstructionsZH_HANS = cocktailCreate.StrInstructionsZH_HANS,
                StrInstructionsZH_HANT = cocktailCreate.StrInstructionsZH_HANT,
                StrDrinkThumb = cocktailCreate.StrDrinkThumb,

                // Aggiungi gli ingredienti e le misure
                StrIngredient1 = cocktailCreate.StrIngredient1,
                StrIngredient2 = cocktailCreate.StrIngredient2,
                StrIngredient3 = cocktailCreate.StrIngredient3,
                StrIngredient4 = cocktailCreate.StrIngredient4,
                StrIngredient5 = cocktailCreate.StrIngredient5,
                StrIngredient6 = cocktailCreate.StrIngredient6,
                StrIngredient7 = cocktailCreate.StrIngredient7,
                StrIngredient8 = cocktailCreate.StrIngredient8,
                StrIngredient9 = cocktailCreate.StrIngredient9,
                StrIngredient10 = cocktailCreate.StrIngredient10,
                StrIngredient11 = cocktailCreate.StrIngredient11,
                StrIngredient12 = cocktailCreate.StrIngredient12,
                StrIngredient13 = cocktailCreate.StrIngredient13,
                StrIngredient14 = cocktailCreate.StrIngredient14,
                StrIngredient15 = cocktailCreate.StrIngredient15,
                StrMeasure1 = cocktailCreate.StrMeasure1,
                StrMeasure2 = cocktailCreate.StrMeasure2,
                StrMeasure3 = cocktailCreate.StrMeasure3,
                StrMeasure4 = cocktailCreate.StrMeasure4,
                StrMeasure5 = cocktailCreate.StrMeasure5,
                StrMeasure6 = cocktailCreate.StrMeasure6,
                StrMeasure7 = cocktailCreate.StrMeasure7,
                StrMeasure8 = cocktailCreate.StrMeasure8,
                StrMeasure9 = cocktailCreate.StrMeasure9,
                StrMeasure10 = cocktailCreate.StrMeasure10,
                StrMeasure11 = cocktailCreate.StrMeasure11,
                StrMeasure12 = cocktailCreate.StrMeasure12,
                StrMeasure13 = cocktailCreate.StrMeasure13,
                StrMeasure14 = cocktailCreate.StrMeasure14,
                StrMeasure15 = cocktailCreate.StrMeasure15
            };

            // Validazione della coerenza tra ingredienti e misure
            var validationError = ValidateIngredientMeasureConsistency(newcocktail);
            if (validationError != null)
            {
                return BadRequest(validationError);
            }

            // Validazione della classe di volume del cocktail
            var volumeError = ValidateVolumeClassCocktail(newcocktail);
            if (volumeError != null)
            {
                return BadRequest(volumeError);
            }
            try{
                _context.DbCocktails.Add(newcocktail);
                await _context.SaveChangesAsync();
                return Ok(new { id = newcocktail.IdDrink, Message = "Cocktail creato con successo !!!" , newcocktail});
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating cocktail : {ex.Message}");
            }
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

            cocktail.StrDrink = updatedCocktail.StrDrink;
            cocktail.StrCategory = updatedCocktail.StrCategory;
            cocktail.StrAlcoholic = updatedCocktail.StrAlcoholic;
            cocktail.StrGlass = updatedCocktail.StrGlass;
            cocktail.StrInstructions = updatedCocktail.StrInstructions;
            cocktail.StrTags = updatedCocktail.StrTags;
            cocktail.PublicCocktail = updatedCocktail.PublicCocktail;
            cocktail.DateModified = DateTime.Now.ToString("yyyy-MM-dd");
            cocktail.StrDrinkThumb = updatedCocktail.StrDrinkThumb;


            // Ingredienti e misure
            for (int i = 1; i <= 15; i++)
            {
                typeof(Cocktail).GetProperty($"StrIngredient{i}")?.SetValue(cocktail, 
                    typeof(CocktailCreate).GetProperty($"StrIngredient{i}")?.GetValue(updatedCocktail));

                typeof(Cocktail).GetProperty($"StrMeasure{i}")?.SetValue(cocktail, 
                    typeof(CocktailCreate).GetProperty($"StrMeasure{i}")?.GetValue(updatedCocktail));
            }

            // Validazione della coerenza tra ingredienti e misure
            var validationError = ValidateIngredientMeasureConsistency(cocktail);
            if (validationError != null)
            {
                return BadRequest(validationError);
            }
            // Validazione della classe di volume del cocktail
            var volumeError = ValidateVolumeClassCocktail(cocktail);
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

            var cocktail = await _context.DbCocktails.Include(c => c.UsersLiked).FirstOrDefaultAsync(c => c.Id == Id && c.PublicCocktail == true);
            if (cocktail == null)
                return NotFound("Cocktail not found.");

            var user = await _context.DbUser.Include(u=> u.CocktailsLike).FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
                return NotFound("User not found.");

            if (user.CocktailsLike.Any(c => c.Id == Id))
            {
                user.CocktailsLike.Remove(cocktail); 
                cocktail.UsersLiked.Remove(user);
                cocktail.Likes = Math.Max(0, cocktail.Likes - 1);
            }
            else
            {
                user.CocktailsLike.Add(cocktail);
                cocktail.UsersLiked.Add(user);
                cocktail.Likes += 1;
            }
            
            await _context.SaveChangesAsync();
            return Ok(new { Message = $"Cocktail like status updated successfully! NameCocktail {cocktail.StrDrink} = {cocktail.Likes} "});
        }


        [HttpGet("GetUserCocktailLikes")]
        public async Task<IActionResult> GetUserCocktailLikes(int id)
        {
            var cocktail = await _context.DbCocktails
                .Include(c => c.UsersLiked)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cocktail == null)
                return NotFound("Cocktail not found.");
            
            var users = cocktail.UsersLiked
                .Select(u => new {
                    u.Id, 
                    u.UserName,
                    u.Name,
                    u.LastName,
                    u.ImgProfileUrl,
                    u.Email 
                })
                .ToList();
           
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
            .Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Name = u.Name,
                LastName = u.LastName,
                ImgProfileUrl = u.ImgProfileUrl ?? string.Empty
            })
            .ToListAsync();

            if (users == null || users.Count == 0)
            {
                users = await _context.DbUser
                    .Where(u => u.UserName.ToLower().Contains(username.ToLower())
                                && u.AcceptCookies == true
                                && u.UserName != userNameFromToken)
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        UserName = u.UserName,
                        Name = u.Name,
                        LastName = u.LastName,
                        ImgProfileUrl = u.ImgProfileUrl ?? string.Empty
                    })
                    .ToListAsync();
            }
            return Ok(users);
        }

        private double ConvertToMilliliters(string? measure)
        {
            if (string.IsNullOrWhiteSpace(measure)) return 0;

            measure = measure.ToLower().Trim();
            double value = 0;

            var parts = measure.Split(' ');
            if (parts.Length == 2)
            {
                // Parse quantity
                if (double.TryParse(parts[0], out double parsed))
                    value = parsed;
                else if (parts[0].Contains('/'))
                {
                    var frac = parts[0].Split('/');
                    if (frac.Length == 2 &&
                        double.TryParse(frac[0], out double num) &&
                        double.TryParse(frac[1], out double denom))
                        value = num / denom;
                }

                // Convert unit
                var unit = parts[1];
                if (unit.Contains("ml")) return value;
                if (unit.Contains("oz")) return value * 29.57;
                if (unit.Contains("cl")) return value * 10;
                if (unit.Contains("cup")) return value * 240;
                if (unit.Contains("tsp")) return value * 5;
                if (unit.Contains("tbsp")) return value * 15;
                if (unit.Contains("dash")) return value * 0.92;
            }
            return 0;
        }

        private string? ValidateVolumeClassCocktail(Cocktail cocktail) {
             double totalMl = 0;
            for (int i = 1; i <= 15; i++){
                var misure = typeof(Cocktail).GetProperty($"StrMeasure{i}")?.GetValue(cocktail)?.ToString();
                totalMl += ConvertToMilliliters(misure);
            }
            var glass  = cocktail.StrGlass ?? "Cocktail glass";
            int maxCapacity = _glassCapacity.TryGetValue(glass, out var ml)? _glassCapacity[glass] : 250;
            if (totalMl > maxCapacity) {
                return $"Il cocktail supera la capacità massima del bicchiere ({maxCapacity} ml).";
            } 
            return null;
        }

        private string? ValidateIngredientMeasureConsistency(Cocktail cocktail)
        {
            for (int i = 1; i <= 15; i++)
            {
                var ingredient = typeof(Cocktail).GetProperty($"StrIngredient{i}")?.GetValue(cocktail)?.ToString();
                var measure = typeof(Cocktail).GetProperty($"StrMeasure{i}")?.GetValue(cocktail)?.ToString();

                if (string.IsNullOrWhiteSpace(ingredient) && !string.IsNullOrWhiteSpace(measure))
                {
                    return $"Errore: la misura {i} è impostata ma manca l'ingrediente corrispondente.";
                }
            }

            return null;
        }


        private int GetSuggestionScore(Cocktail c, User user, List<string> searchHistory, List<Cocktail> likedCocktails)
        {
            int score = 0;

            var filterWeight = new Dictionary<SuggestionUser, int>{
                { SuggestionUser.NameMatch, 4 },
                { SuggestionUser.IngredientMatch, 2 },
                { SuggestionUser.CategoryMatch, 2 },
                { SuggestionUser.GlassMatch, 3 },
                { SuggestionUser.DescriptionMatch, 2 },
                { SuggestionUser.SearchHistoryMatch, 6 },
                { SuggestionUser.likeCocktail, 7 }
            };


            var ingredients = Enumerable.Range(1, 15)
                .Select(i => typeof(Cocktail).GetProperty($"StrIngredient{i}")?.GetValue(c)?.ToString()?.ToLower())
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .ToList();


            // Ingredienti più presenti nei like
            var topIngredientLikes = likedCocktails
                .SelectMany(l => Enumerable.Range(1, 15)
                    .Select(i => typeof(Cocktail).GetProperty($"StrIngredient{i}")?.GetValue(l)?.ToString()?.ToLower())
                    .Where(i => !string.IsNullOrWhiteSpace(i)))
                .GroupBy(i => i)
                .OrderByDescending(g => g.Count())
                .Take(5) // Prendi i primi 3 ingredienti più comuni
                .Select(g => g.Key)
                .ToList();

            if(ingredients.Any(i => topIngredientLikes.Contains(i))) 
                score += filterWeight[SuggestionUser.IngredientMatch];
            // MATCH con LIKE
            foreach (var liked in likedCocktails)
            {
                if (liked.Id == c.Id) {
                    score += filterWeight[SuggestionUser.likeCocktail];
                    continue; 
                }
                // Ingredient match
                var likedIngredients = Enumerable.Range(1, 15)
                    .Select(i => typeof(Cocktail).GetProperty($"StrIngredient{i}")?.GetValue(liked)?.ToString()?.ToLower())
                    .Where(i => !string.IsNullOrWhiteSpace(i))
                    .ToList();

                if (ingredients.Any(i => likedIngredients.Contains(i))) 
                    score +=  filterWeight[SuggestionUser.IngredientMatch];

                // Categoria, bicchiere
                if (!string.IsNullOrEmpty(c.StrCategory) && c.StrCategory == liked.StrCategory)
                    score +=  filterWeight[SuggestionUser.CategoryMatch];;
                if (!string.IsNullOrEmpty(c.StrGlass) && c.StrGlass == liked.StrGlass)
                    score +=  filterWeight[SuggestionUser.GlassMatch];
            }

            // MATCH con ricerche recenti
            if (searchHistory.Any(s => !string.IsNullOrEmpty(c.StrDrink) && c.StrDrink.ToLower().Contains(s)))
                score +=  filterWeight[SuggestionUser.NameMatch];
            if (searchHistory.Any(s => ingredients.Any(i => i != null && i.Contains(s))))
                score +=  filterWeight[SuggestionUser.IngredientMatch];
            if (searchHistory.Any(s => c.StrCategory?.ToLower().Contains(s) == true))
                score +=  filterWeight[SuggestionUser.CategoryMatch];
            if (searchHistory.Any(s => c.StrGlass?.ToLower().Contains(s) == true))
                score +=  filterWeight[SuggestionUser.GlassMatch];
            if (searchHistory.Any(s => c.StrInstructions?.ToLower().Contains(s) == true))
                score +=  filterWeight[SuggestionUser.DescriptionMatch];
            return score;
        }
    }
}
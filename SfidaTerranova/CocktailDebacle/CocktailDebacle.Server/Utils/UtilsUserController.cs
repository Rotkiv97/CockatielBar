using System;
using System.Collections.Generic;
using System.Linq;
using CocktailDebacle.Server.Service;
using System.Threading.Tasks;
using CocktailDebacle.Server.Models;
using CocktailDebacle.Server.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CocktailDebacle.Server.Utils
{
    public static class UtilsUserController
    {
        public static UserDto UserToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Name = user.Name,
                LastName = user.LastName,
                Email = user.Email,
                ImgProfileUrl = user.ImgProfileUrl ?? string.Empty,
                Followed_Users = user.Followed_Users.Select(u => u.Id).ToList(),
                Followers_Users = user.Followers_Users.Select(u => u.Id).ToList(),
            };
        }

        public static Task<List<Cocktail>> FindSimilarCocktails(IEnumerable<Cocktail> baseCocktails, IEnumerable<Cocktail> allCocktails, int limit)
        {
            var keywords = baseCocktails
                .SelectMany(c => new[]
                {
                    c.StrDrink, c.StrGlass, c.StrAlcoholic, c.StrCategory
                }.Concat(GetIngredients(c)))
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(s => s!.ToLower())
                .Distinct()
                .ToList();

            var matched = allCocktails
                .Where(c =>
                    keywords.Contains(c.StrDrink?.ToLower() ?? string.Empty) ||
                    keywords.Contains(c.StrGlass?.ToLower() ?? string.Empty) ||
                    keywords.Contains(c.StrAlcoholic?.ToLower() ?? string.Empty) ||
                    keywords.Contains(c.StrCategory?.ToLower() ?? string.Empty) ||
                    GetIngredients(c).Any(i => keywords.Contains(i.ToLower()))
                )
                .OrderBy(_ => Guid.NewGuid())
                .Take(limit)
                .ToList();

            return Task.FromResult(matched);
        }

        public static List<string> GetIngredients(Cocktail c) =>
            new[]
            {
                c.StrIngredient1, c.StrIngredient2, c.StrIngredient3, c.StrIngredient4, c.StrIngredient5,
                c.StrIngredient6, c.StrIngredient7, c.StrIngredient8, c.StrIngredient9, c.StrIngredient10,
                c.StrIngredient11, c.StrIngredient12, c.StrIngredient13, c.StrIngredient14, c.StrIngredient15
            }.Where(s => !string.IsNullOrEmpty(s)).Cast<string>().ToList();

        public static async Task<List<Cocktail>> GetSearchHistory(AppDbContext context, int userId)
        {
            var searchTerms = await context.DbUserHistorySearch
                .Where(h => h.UserId == userId && h.SearchText != null)
                .OrderByDescending(h => h.SearchDate)
                .Select(h => h.SearchText!.ToLower())
                .Distinct()
                .Take(20)
                .ToListAsync();

            var cocktails = await context.DbCocktails.ToListAsync();

            return cocktails
                .Where(c =>
                    searchTerms.Any(term =>
                        (!string.IsNullOrEmpty(c.StrDrink) && c.StrDrink.ToLower().Contains(term)) ||
                        (!string.IsNullOrEmpty(c.StrCategory) && c.StrCategory.ToLower().Contains(term)) ||
                        (!string.IsNullOrEmpty(c.StrGlass) && c.StrGlass.ToLower().Contains(term)) ||
                        (!string.IsNullOrEmpty(c.StrAlcoholic) && c.StrAlcoholic.ToLower().Contains(term)) ||
                        GetIngredients(c).Any(i => i.ToLower().Contains(term))
                    )
                )
                .ToList();
        }

        public static async Task<string> GetPreferredCategory(AppDbContext context, int userId)
        {
            var likeCategories = await context.DbCocktails
                .Where(c => c.UserLikes.Any(u => u.Id == userId) && !string.IsNullOrEmpty(c.StrCategory))
                .Select(c => c.StrCategory!.ToLower())
                .ToListAsync();

            var historyTerms = await context.DbUserHistorySearch
                .Where(h => h.UserId == userId && h.SearchText != null)
                .Select(h => h.SearchText!.ToLower())
                .ToListAsync();

            var allCocktails = await context.DbCocktails
                .Where(c => !string.IsNullOrEmpty(c.StrCategory))
                .ToListAsync();

            if (!likeCategories.Any() && !historyTerms.Any())
            {
                return allCocktails
                    .GroupBy(c => c.StrCategory!.ToLower())
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .FirstOrDefault() ?? string.Empty;
            }

            var categoriesFromHistory = allCocktails
                .Where(c => historyTerms.Any(term =>
                    (!string.IsNullOrEmpty(c.StrCategory) && c.StrCategory.ToLower().Contains(term)) ||
                    (!string.IsNullOrEmpty(c.StrDrink) && c.StrDrink.ToLower().Contains(term)) ||
                    (!string.IsNullOrEmpty(c.StrGlass) && c.StrGlass.ToLower().Contains(term)) ||
                    (!string.IsNullOrEmpty(c.StrAlcoholic) && c.StrAlcoholic.ToLower().Contains(term)) ||
                    GetIngredients(c).Any(i => i.ToLower().Contains(term))
                ))
                .Select(c => c.StrCategory!.ToLower())
                .ToList();

            var allCategories = likeCategories
                .Concat(categoriesFromHistory)
                .Where(c => !string.IsNullOrEmpty(c))
                .ToList();

            if (!allCategories.Any())
            {
                return allCocktails
                    .GroupBy(c => c.StrCategory!.ToLower())
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .FirstOrDefault() ?? string.Empty;
            }

            return allCategories
                .GroupBy(c => c)
                .OrderByDescending(g => g.Count())
                .First()
                .Key;
        }
    }
}

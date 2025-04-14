using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using OpenAI_API.Embedding;
using CocktailDebacle.Server.Service;
using CocktailDebacle.Server.Models;

namespace CocktailDebacle.Server.Service
{
    public class  RecommenderEngine
    {
        private readonly OpenAIService _openAIService;

        public RecommenderEngine(OpenAIService openAIService)
        {
            _openAIService = openAIService;
        }

        public async Task<float[]> generateEmbedding(string input)
        {
            var output = await _openAIService.GetEmbeddingAsync(input);
            return output;
        }

        public async Task<List<(Cocktail, double)>> RanckCocktailAsync(float[] userVector, List<Cocktail> cocktails)
        {
            var rankedCocktails = new List<(Cocktail, double)>();

            foreach (var cocktail in cocktails)
            {
                var text = $"{cocktail.StrDrink},{cocktail.StrCategory},{cocktail.StrAlcoholic},{cocktail.StrGlass},{cocktail.StrInstructions}";
                var cocktailVector = await _openAIService.GetEmbeddingAsync(text);
                var similarity = RecommenderSystemsUtils.CosineSimilarity(userVector, cocktailVector);
                rankedCocktails.Add((cocktail, similarity));
            }

            return rankedCocktails.OrderByDescending(x => x.Item2).ToList();
        }
    }
}
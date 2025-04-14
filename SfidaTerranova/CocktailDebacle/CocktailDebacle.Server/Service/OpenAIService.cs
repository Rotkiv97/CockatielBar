using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using OpenAI_API.Embedding;
using OpenAI_API.Images;
using Microsoft.AspNetCore.Components.Forms;

namespace CocktailDebacle.Server.Service
{
    public class OpenAIService
    {
        private readonly OpenAIAPI _api;

        public OpenAIService(string apiKey)
        {
            _api = new OpenAIAPI(apiKey);
        }

        public async Task<float[]> GetEmbeddingAsync(string input){
            var output = await _api.Embeddings.CreateEmbeddingAsync(new EmbeddingRequest
            {
                Input = input,
                Model = "Text-Embedding-ADA-002"
            });
            return output.Data[0].Embedding.ToArray();
        }
    }
}
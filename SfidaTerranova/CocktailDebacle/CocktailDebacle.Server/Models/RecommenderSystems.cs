namespace CocktailDebacle.Server.Models
{
    public class RecommenderSystems
    {
            public int Id { get; set; }
            public int UserId { get; set; }
            public User? User { get; set; } 

            public string ProfileText { get; set; } = string.Empty; // Testo del profilo dell'utente es: "Vodka, Lime Juice, Strong, Fruity, Cocktail, Sweet"
            public string? VectorJsonEmbedding { get; set; } // Rappresentazione vettoriale dell'utente in formato JSON
            public DateTime LastUpdated { get; set; } // Data dell'ultimo aggiornamento del profilo
    }

    public class RecommenderSystemsUtils{
        public static double CosineSimilarity(float[] vectorA, float[] vectorB)
        {
            if (vectorA.Length != vectorB.Length)
                throw new ArgumentException("I vettori devono avere la stessa lunghezza.");

            float dotProduct = 0.0f;
            float magnitudeA = 0.0f;
            float magnitudeB = 0.0f;

            for (int i = 0; i < vectorA.Length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];
                magnitudeA += vectorA[i] * vectorA[i];
                magnitudeB += vectorB[i] * vectorB[i];
            }

            if (magnitudeA == 0 || magnitudeB == 0)
                return 0.0;

            return dotProduct / (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
        }
    }
}

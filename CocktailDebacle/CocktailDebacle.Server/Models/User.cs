namespace CocktailDebacle.Server.Models
{
    public class User
    {
        //Informazioni primarie dell'utente
        public int Id { get; set; } = 0;
        public string UserName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string EmailConfirmed { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PasswordConfirmed { get; set; } = string.Empty;


        //Permessi
        public bool PersonalizedExperience { get; set; } = false;
        public bool AcceptCookis { get; set; } = false;

        // Stato
        public bool online { get; set; } = false;

        // gestione preferenze e ricerca intelligente personalizzata
        public ICollection<User> Friends { get; set; } = new List<User>();
        public ICollection<Cocktail> CocktailsLike { get; set; } = new List<Cocktail>();
        public ICollection<Cocktail> CocktailsCreate { get; set; } = new List<Cocktail>();
        public RecommenderSystems RecommenderSystems { get; set; } = new RecommenderSystems();

        // Pesonalizzazioni 
        public string Leanguage { get; set; } = string.Empty;
        public string ImgProfile {  get; set; } = string.Empty;
    }
}

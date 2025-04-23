namespace CocktailDebacle.Server.DTOs
{
    public class RegisterUserDto
    {
        public string UserName { get; set; } = string.Empty; // Inizializzato con valore predefinito
        public string Name { get; set; } = string.Empty; // Inizializzato con valore predefinito
        public string LastName { get; set; } = string.Empty; // Inizializzato con valore predefinito
        public string Email { get; set; } = string.Empty; // Inizializzato con valore predefinito
        public string PasswordHash { get; set; } = string.Empty; // Inizializzato con valore predefinito

        public List<string> ingredeients { get; set; } = new List<string>(); // Inizializzato con una lista vuota
        public List<string> measures { get; set; } = new List<string>(); // Inizializzato con una lista vuota
        public bool AcceptCookies { get; set; } // Campo booleano, non richiede inizializzazione
    }
}
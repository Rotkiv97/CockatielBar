# Architettura e Infrastruttura di CocktailDebacle

In questo documento spiego **l'architettura del progetto CocktailDebacle**, la logica dietro le scelte strutturali, il funzionamento del server, la comunicazione con il database e l'integrazione con il frontend.

---

## Obbiettivo dell'Architettura e Infrastruttura

L'obiettivo è creare un **backend solido, sicuro e scalabile**, supportare funzionalità utente, gestione di contenuti multimediali e raccomandazioni personalizzate.

---

## Architettura del Database

### Entità principali

- `User`: rappresenta un utente registrato
- `Cocktail`: rappresenta un cocktail, sia importato che creato dagli utenti
- `UserHistorySearch`: storico delle ricerche dell’utente usato
- Tabelle relazionali `UserCocktailsLike`, `UserUser` (followeds/followers)

### Relazioni

- `User` <-> `Cocktail`: molti-a-molti per i like
- `User` <-> `User`: molti-a-molti per il follow
- `User` <-> `UserHistorySearch`: uno-a-molti

---

## Connessione al Database

### DbContext

```csharp
public class AppDbContext : DbContext
{
    public DbSet<User> DbUser { get; set; }
    public DbSet<Cocktail> DbCocktails { get; set; }
    public DbSet<UserHistorySearch> DbUserHistorySearch { get; set; }
}
```

### Retry Policy (Program.cs)

```csharp
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(30)
    ));
```

Evita crash all’avvio quando il DB impiega tempo a inizializzarsi.

---

## Sicurezza: JWT & Validazione

### JWT Authentication

Ogni utente ottiene un token dopo il login tramite `IAuthService`:

```csharp
ClaimTypes.Name = user.UserName
JwtRegisteredClaimNames.Sub = user.Id
```

### Pulizia automatica dei token

Tramite `CleanTokenHostedService` i token scaduti vengono invalidati ogni 30 secondi.

---

## Struttura del Backend

### Layer di Servizi

- `AuthService`: gestione login, creazione token
- `CloudinaryService`: gestione immagini (profilo, cocktail)
- `CocktailImportService`: importazione dati esterni
- `TranslationService`: traduzione contenuti
- `ICleanTokenHostedService`: gestione async e pulizia token

Tutti i servizi sono registrati in `Program.cs`.

---

## Comunicazione Server <-> Frontend

### CORS + Token

```csharp
builder.WithOrigins("http://localhost:4200")
       .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
```

### Login

1. L’utente si autentica (`/api/Users/login`)
2. Il server restituisce il token JWT
3. Il token viene inviato come `Authorization: Bearer <token>` in ogni richiesta
4. Il middleware del server valida e rigenera i dati utente

---

## API & Controller

### UsersController

- Login / Logout
- Registrazione
- Modifica profilo
- Upload immagini profilo
- Gestione relazioni follow/unfollow
- Recupero cocktail preferiti e suggeriti

### CocktailsController

- Ricerca avanzata (con filtri)
- Gestione cocktail personalizzati
- Upload immagini cocktail
- Funzionalità social (like, creator, suggerimenti)

---

## DTO & Sicurezza

Per evitare di esporre dati sensibili (es. `PasswordHash`, `Token`) usiamo DTO (Data Transfer Object):

```csharp
public class UserDto {
    public int Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    ecc...
}
```

---

## Servizi Esterni Integrati

- **Cloudinary**: Hosting immagini sicuro
- **Azure Translator**: Traduzione multilingua
- **CocktailDB API**: Importazione contenuti

---

## Performance & Scalabilità

- `DbContext` gestito con scope per ridurre memory leaks
- Hosted Services asincroni
- Retry Policy sulle connessioni SQL
- Pagination lato API per ricerche

---

Visione Generale:

| Specifica            | Vantaggio                                 |
|----------------------|-------------------------------------------|
| Sicurezza            | JWT + Token Expiry                        |
| Scalabilità          | HostedService, Retry, Scoped DI           |
| User Experience      | Traduzione dinamica, suggerimenti social  |
| Modularità           | Controller, DTO, Service separati         |
| Performance          | Paginazione, filtraggio lato server       |
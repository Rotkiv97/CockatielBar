# Gestione Token e Servizi – CocktailDebacle

## Architettura dei Servizi

## `IAuthService` & `AuthService`

#### Perché è stato creato

- Isolare la logica di autenticazione dal controller
- Rendere il codice testabile e mantenibile
- Supportare l’iniezione delle dipendenze

#### `AuthService`

- Verifica le credenziali
- Genera un JWT
- Imposta la scadenza
- Registra il token nel database
- Traccia il token per l’invalidazione automatica

---

## `CleanTokenHostedService`

### Utilizzo

- Invalida i token scaduti
- Pulisce i token dal database
- Protegge l'accesso con token scaduti/rubati

#### Funzionamento

- All’avvio: rimuove token scaduti
- Ogni 30s: controlla e rimuove quelli che sono scaduti
- Coda prioritaria: gestisce chi ha token in scadenza
- Metodo `TrackToken()`: registra nuovi token generati

---

## Integrazione in `Program.cs`

### JWT Setup

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        ...
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(token)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                var rawToken = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                var user = await dbContext.DbUser.FirstOrDefaultAsync(u => u.Token == rawToken);

                if (user == null || user.TokenExpiration <= DateTime.UtcNow)
                {
                    context.Fail("Token not valid anymore.");
                }
            }
        };
    });
```

### Registrazione Servizi

```csharp
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<ICleanTokenHostedService, CleanTokenHostedService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<ICleanTokenHostedService>());
```

### Middleware personalizzato

Controllo manuale del token per endpoint `[Authorize]`:

```csharp
app.Use(async (context, next) => {
    var endpoint = context.GetEndpoint();
    if(endpoint?.Metadata.GetMetadata<IAuthorizeData>() != null)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        if (!string.IsNullOrEmpty(token))
        {
            var Dbcontext = context.RequestServices.GetRequiredService<AppDbContext>();
            var user = await Dbcontext.DbUser.FirstOrDefaultAsync(u => u.Token == token);
            if (user == null || user.TokenExpiration == null || user.TokenExpiration <= DateTime.UtcNow)
            {
                context.Response.StatusCode = 401;
                return;
            }
        }
    }
    await next();
});
```

---

## Ciclo di Vita di un Token

```
[Login Request]
      |
      v
[AuthService.AuthenticateUser()]
      |
      v
[JWT Token Creato]
      |
      |---> [Salvato nel DB]
      |---> [TokenExpiration impostata]
      |---> [Tracciato da CleanTokenHostedService]
      |
      v
[Client usa il token]
      |
      v
[Richiesta a Endpoint Protetto]
      |
      |---> [JwtBearer + Middleware personalizzato]
      |---> [Verifica DB: token valido e non scaduto]
             |
             +--> NO → 401 Unauthorized
             |
             +--> SÌ → Accesso consentito
```

---

## I Claim nel JWT

### Cosa sono i Claim?

I **claim** sono informazioni inserite nel payload del token JWT e rappresentano attributi dell'utente autenticato. Sono usati per identificare l’utente, fornire contesto, e autorizzare azioni.

### 📌 Claim usati in `AuthService`

```csharp
var claims = new List<Claim>
{
    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
    new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
    new Claim(ClaimTypes.Name, user.UserName),
    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
};
```

- `sub`: Identificativo univoco dell’utente (ID)
- `unique_name`: Username dell’utente
- `name`: Nome leggibile (può coincidere con username)
- `jti`: ID univoco del token (per prevenzione replay)
- `iat`: Issued At Time – timestamp di creazione del token

**In aggiunta:**

```csharp
if (!string.IsNullOrEmpty(user.Email))
    claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
```

- `email`: opzionale, nel nostro caso non se ne fa uso

### Perché li abbiamo usati

- Per identificare univocamente l’utente (es. `UniqueName`)
- Per includere informazioni che possono essere usate nei controller o policy di autorizzazione
- Per supportare logging, tracciabilità

---

## Configurazione in `appsettings.json`

Nel file `appsettings.json` sono definiti i parametri utilizzati per firmare e validare i token JWT.

### Configurazione

```json
"Jwt": {
  "Key": "super-secret-key-that-is-long-enough-123!",
  "Issuer": "CocktailDebacle",
  "Audience": "CocktailDebacleUsers"
}
```

### Campi

| Campo     | Descrizione |
|-----------|-------------|
| `Key`     | Chiave segreta usata per firmare il token. Deve essere lunga e sicura. |
| `Issuer`  | Identifica chi ha emesso il token  |
| `Audience`| Indica chi può accettare il token |

### 🧩 Utilizzo nel codice

Nel `Program.cs` e in `AuthService` la chiave e gli altri valori vengono letti tramite `IConfiguration`:

```csharp
var jwtKey = _configuration["Jwt:Key"];
var issuer = _configuration["Jwt:Issuer"];
var audience = _configuration["Jwt:Audience"];
```

Questi valori vengono poi usati per generare e validare i token JWT in modo coerente e centralizzato.

---

## Vantaggi

| Caratteristica | Beneficio |
|----------------|-----------|
| Salvataggio Token | Possibilità di revoca lato server |
| TokenExpiration | Controllo preciso sul tempo di vita |
| `TrackToken()` | Pulizia automatica e ordinata |
| Middleware Custom | Protezione aggiuntiva rispetto a solo JWT |
| Interfaccia `IAuthService` | Codice più testabile e modulare |

---

## Riassunto

Questa implementazione bilancia l’efficienza dello standard JWT con un controllo server-side robusto, ideale per ambienti dove la revoca dei token e la sicurezza temporale sono importanti. `IAuthService` e `CleanTokenHostedService` consentono un sistema scalabile, estendibile e sicuro per la gestione dell’autenticazione utente.

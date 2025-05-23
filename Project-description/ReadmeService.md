# Servizi Custom in CocktailDebacle

In questo documento descrivo i tre servizi utilizzati nel backend del progetto **CocktailDebacle**:

- `TranslationService`
- `CloudinaryService`
- `CocktailImportService`

---

## TranslationService

Permette la **traduzione automatica del testo** in diverse lingue tramite le API Cognitive Services di Azure. È utile per rendere l'app multilingua, permettendo agli utenti di ricevere contenuti nella lingua desidertata.

### Configurazione Cloudinary

Nel file `appsettings.json`:

```json
"TranslatorService": {
  "SubscriptionKey": "...",
  "Endpoint": "https://translator-cocktaildebacle.cognitiveservices.azure.com/",
  "Region": "westeurope"
}
```

### Come Funziona

- Valida le lingue supportate (`it`, `en`, `fr`, ecc.)
- Invia una POST all’endpoint Azure con il testo
- Gestisce errori e risposte JSON
- Restituisce la traduzione testuale

#### Metodo Principale

```csharp
public async Task<string> TranslateTextAsync(string text, string toLanguage, string? fromLanguage = null)
```

- Valida input e lingue
- Recupera configurazioni da `appsettings.json` e variabili d’ambiente
- Prepara corpo e intestazioni della richiesta
- Invia POST verso Azure Translator API
- Parsa la risposta JSON per restituire il testo tradotto

#### Gestione JSON

```csharp
public class TranslationResult
{
    [JsonPropertyName("translations")]
    public List<TranslationText>? Translations { get; set; }
}

public class TranslationText
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("to")]
    public string? To { get; set; }
}
```

> Usato per mappare correttamente la risposta dell'API Microsoft Azure Translator.

### Vantaggi dell'uso

- Perchè offre contenuti multilingua (cocktail, descrizioni, ecc.)
- Perchè offre supporta un’interfaccia globale
- Perchè offre una centralizzazione della logica di traduzione e configurazione

---

## CloudinaryService

Gestisce il **caricamento e la gestione delle immagini** su Cloudinary, utile per salvare immagini utente, immagini di cocktail, in modo che il progetto sia il più leggero possibile.

### Configurazione

Nel file `appsettings.json`:

```json
"CloudinarySettings": {
  "CloudName": "...",
  "ApiKey": "...",
  "ApiSecret": "..."
}
```

### Funzionalità

- `UploadImageAsync`: carica immagine da file
- `UploadImageAsyncUrl`: carica immagine da URL
- `GeneratePrivateImageUrl`: genera URL autenticato
- `DeleteImageAsync`: elimina immagine su richiesta

#### Costruttore

```csharp
public CloudinaryService(IOptions<CloudinarySettings> cloudinaryConfig)
```

- Inizializza un oggetto `Cloudinary` con i parametri da `appsettings.json`

#### Upload da file

```csharp
public async Task<string?> UploadImageAsync(IFormFile file, string publicId)
```

- Carica uno stream dell’immagine
- Costruisce i parametri con tipo `authenticated`
- Usa `UploadAsync` per salvare nel cloud

#### Upload da URL

```csharp
public async Task<string?> UploadImageAsyncUrl(string url, string publicId)
```

- Usa un URL esterno per caricare l’immagine

#### URL firmato per accesso privato

```csharp
public string GeneratePrivateImageUrl(string publicId, int expireSeconds = 3600)
```

- Restituisce un URL temporaneo valido solo per un’ora

#### Eliminazione immagine

```csharp
public async Task<string> DeleteImageAsync(string publicId)
```

- Rimuove l’immagine tramite `DestroyAsync`, usata per la modifica delle immagini del profilo.

### Perché lo usiamo

- Per non salvare immagini nel file system locale
- Per gestire immagini con accesso privato e controllato

---

## CocktailImportService

Importa automaticamente cocktail da [TheCocktailDB API](https://www.thecocktaildb.com/api.php), popolando il database all'avvio del programma.

### Funzionamento

- Scorre tutte le lettere dell’alfabeto + numeri
- Fa richieste `GET` per ogni lettera (`search.php?f=a`)
- Converte la risposta JSON in oggetti `Cocktail`
- Salva nel database se non è già presente

```csharp
public async Task ImportCocktailsAsync()
```

#### Normalizzazione dati

```csharp
private void CheckCocktails(Cocktail drink)
```

- Assicura che i campi stringa non siano `null`
- Sostituisce con `string.Empty` per evitare errori futuri

> Tutto il processo è asincrono e include `await Task.Delay(1000)` per evitare throttling sull’API remota.

### Uso in `Program.cs`

```csharp
var cocktailImportService = services.GetRequiredService<CocktailImportService>();
await cocktailImportService.ImportCocktailsAsync();
```

---

Questi tre servizi arricchiscono il backend di CocktailDebacle fornendo:

| Servizio             | Utilità principale                         |
|----------------------|--------------------------------------------|
| `TranslationService` | Traduzione automatica del contenuto        |
| `CloudinaryService`  | Gestione professionale di immagini         |
| `CocktailImportService` | Importazione dinamica di dati di cocktail |

Sono completamente configurabili e facilmente estendibili per adattarsi a future esigenze del progetto.
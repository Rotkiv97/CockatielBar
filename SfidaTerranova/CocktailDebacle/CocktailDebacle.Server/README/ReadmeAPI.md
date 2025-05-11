# API del progetto e come testarle su Postman

![Img](./src/Postman.png)

# Inserise i Token su postoman

# Autenticazione
## Alcune API richiedono un token JWT. Per aggiungerlo in **Postman**:
- Vai nella sezione `Authorization`
- Imposta `Auth Type` su **Bearer Token**
- Inserisci il token dell’utente nel campo `Token`

![Img](./src/TokenPostman.png)

## 🍸 API Cocktail
- [GET - http://localhost:5052/api/Cocktails/cocktails](#get-all-cocktail)
- [GET - http://localhost:5052/api/Cocktails/cocktail/by-id](#cocktail-by-id)
- [GET - http://localhost:5052/api/Cocktails/search](#my-cocktails)
- [GET - http://localhost:5052/api/Cocktails/IngedientSearch/SearchIngredient](#ingredient-search)
- [GET - http://localhost:5052/api/Cocktails/SearchMeasureType/searchMeasure](#search-measure)
- [GET - http://localhost:5052/api/Cocktails/SearchGlass/searchGlass](#search-glass)
- [GET - http://localhost:5052/api/Cocktails/SearchCategory/searchCategory](#)
- [GET - http://localhost:5052/api/Cocktails/GetUserCocktailLikes](#)
- [GET - http://localhost:5052/api/Cocktails/GetCountCocktailLikes/{id}](#)
- [GET - http://localhost:5052/api/Cocktails/ingredients](#)
- [GET - http://localhost:5052/api/Cocktails/SearchUser/{username}](#)

- [POST - http://localhost:5052/api/Cocktails/CocktailCreate](#)
- [POST - http://localhost:5052/api/Cocktails/{id}/UploadImageCocktail-local](#)
- [POST - http://localhost:5052/api/Cocktails/{id}/UploadImageCocktail-url](#)

- [PUT - http://localhost:5052/api/Cocktails/CocktailUpdate/{idDrink}](#)

- [DELETE - http://localhost:5052/api/Cocktails/CocktailDelete/{idDrink}](#)

## 👤 API Users
- [GET - http://localhost:5052/api/Users/GetUser/{username}](#)
- [GET - http://localhost:5052/api/Users/check-token](#)
- [GET - http://localhost:5052/api/Users/GetToken](#)
- [GET - http://localhost:5052/api/Users/getPassword/{id}](#)
- [GET - http://localhost:5052/api/Users/GetMyCocktailLike/{id}](#)
- [GET - http://localhost:5052/api/Users/GetFollowedUsers/{id}](#)
- [GET - http://localhost:5052/api/Users/GetFollowersUsers/{id}](#)
- [GET - http://localhost:5052/api/Users/Get_Cocktail_for_Followed_Users](#)
- [GET - http://localhost:5052/api/Users/ThisYourCocktailLike/{id}](#)
- [GET - http://localhost:5052/api/Users/SuggestionsCocktailByUser/{id}](#)

- [POST - http://localhost:5052/api/Users/login](#)
- [POST - http://localhost:5052/api/Users/logout](#)
- [POST - http://localhost:5052/api/Users/register](#)
- [POST - http://localhost:5052/api/Users/upload-profile-image-local/{id}](#)
- [POST - http://localhost:5052/api/Users/upload-profile-image-Url/{id}](#)
- [POST - http://localhost:5052/api/Users/FollowedNewUser/{followedUserId}](#)

- [PUT - http://localhost:5052/api/Users/{id}](#)

- [DELETE - http://localhost:5052/api/Users/{id}](#)

## 🌍 API Translation
- [POST - http://localhost:5052/api/Translation/translate](#)


### Get All Cocktail

Restituisce **tutti i cocktail** presenti nel database come lista di oggetti `DTO`.

---

### Cocktail By Id

Restituisce un singolo cocktail, identificato da `id`.  
Se l’utente è autenticato e ha accettato i cookie, la ricerca viene **salvata nello storico personale** (`UserHistorySearch`).
Esempio:`/api/Cocktails/cocktail/by-id?id=12`

---

### Search

Permette la ricerca avanzata di cocktail tramite **query param**:

| Parametro     | Descrizione                                  |
|---------------|----------------------------------------------|
| nameCocktail  | Nome del cocktail                            |
| UserSearch    | Username dell’utente creatore del cocktail   |
| glass         | Tipo di bicchiere                            |
| ingredient    | Ingrediente incluso                          |
| category      | Categoria del cocktail                       |
| alcoholic     | Alcoholic / Non Alcoholic / Optional alcohol |
| page          | Numero della pagina                          |
| pageSize      | Elementi per pagina                          |

> Può essere usata con uno o più filtri. Se viene usato solo `UserSearch`, restituisce **una lista di utenti**.

Esempio: `/api/Cocktails/search?ingredient=vodka&page=1&pageSize=10`

---

### My Cocktails

Restituisce tutti i cocktail creati dall'utente attualmente autenticato.
Richiede [Token](#inserise-i-token-su-postoman).

---

### Ingredient Search

Suggerisce ingredienti filtrati in base alla stringa `ingredient`.  
Il risultato dipende dall'età dell’utente (`IsOfMajorityAge`):  
- se sei minorenne: solo ingredienti analcolici
- se sei maggiorenne: tutti

Esempio: `/api/Cocktails/IngredientSearch/SearchIngredient?id={userId}&ingredient={string}`

---

### Search Measure

Suggerisce le unità di misura supportate (es: `ml`, `oz`, `dash`...).  
Filtro opzionale con il parametro `measure`.
Richiede [Token](#inserise-i-token-su-postoman).

Esempio: `/api/Cocktails/SearchMeasureType/searchMeasure?id={userId}&measure={string}`

---

### Search Glass

Suggerisce i tipi di bicchieri disponibili nel database filtrando per nome.
Richiede [Token](#inserise-i-token-su-postoman).

Esempio: `/api/Cocktails/SearchGlass/searchGlass?id={userId}&glass=margarita`

---

### Search Category

Suggerisce le categorie esistenti (es: "Cocktail", "Soft Drink", ecc.), con supporto alla ricerca.
Richiede [Token](#inserise-i-token-su-postoman).

Esempio: `/api/Cocktails/SearchCategory/searchCategory?id={userId}&category=soft`

---

### Get User Cocktail Likes

Restituisce l’elenco degli utenti che hanno messo “like” al cocktail specificato.

Esempio: `/api/Cocktails/GetUserCocktailLikes?id={cocktailId}`

---

### Get Count Cocktail Likes

Restituisce il **numero totale di like** per un cocktail.

Esempio: `/api/Cocktails/GetCountCocktailLikes/{id}`

---

### Ingredients

Restituisce l’elenco **unico e ordinato** di tutti gli ingredienti usati nei cocktail presenti nel database.

Esempio: `/api/Cocktails/ingredients`

---

### Search User

Ricerca utenti filtrando per `username`.  
Funziona solo se sei autenticato [Token](#inserise-i-token-su-postoman).  
Restituisce utenti che iniziano o contengono il nome cercato (escludendo te stesso).

Esempio: `/api/Cocktails/SearchUser/{username}`

---

### Cocktail Create

Permette di **creare un nuovo cocktail**.  
Valida:
- Età (non alcolici se sei minorenne)
- Consistenza tra ingredienti e misure
- Volume massimo consentito in base al bicchiere scelto

Richiede [Token](#inserise-i-token-su-postoman) e JSON del cocktail (`CocktailCreate` DTO).

Esempio: `/api/Cocktails/CocktailCreate`

---

### Upload Image Cocktail local

Carica un'immagine locale per il cocktail (Cloudinary).  
Ed ha bisono del fili caricato [FromFile] file.

Esempio: `/api/Cocktails/{id}/UploadImageCocktail-local`

---

### Upload Image Cocktail Url

Carica un'immagine da URL remoto (es: `https://...`) per un cocktail.
Sostituisce l'immagine precedente se presente.

Esempio: `/api/Cocktails/{id}/UploadImageCocktail-url`

---

### Like

Permette di aggiungere o rimuovere un like a un cocktail.  
Restituisce il numero aggiornato di like.
A bisono del [Token](#inserise-i-token-su-postoman).

Esempio: `/api/Cocktails/like/{id}`

### Cocktail Update

Permette di modificare un cocktail **solo se sei l’autore**.  
Validazione come la creazione (alcolici, misure, volume...).
A bisono del [Token](#inserise-i-token-su-postoman).

Esempio: `/api/Cocktails/CocktailUpdate/{idDrink}`

---

### Cocktail Delete

Elimina un cocktail.  
Accessibile solo dall’utente autore del cocktail.
A bisono del [Token](#inserise-i-token-su-postoman).

Esempio: `/api/Cocktails/CocktailDelete/{idDrink}`



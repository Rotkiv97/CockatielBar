# Database
AppDbcontext
  - Users()
  - cocktail pubblic
     - list<cocktail> 
     - valutazione o star
     - visite 
## modelli Utils
- User
   - esperienza personazillata bool(locazione(ita, frencia, ecc), ) 
   - online bool
   - Preferiti list<>
   - Name string
   - lastname string
   - Username string
   - password(criptata)
   - imail string
   - ImgProfilo string
   - Accept cookis bool
   - Leanguage
   - list cocktailcreati(public, private);
      - public bool
      - name
      - description
      - ingredienti
      - Immagine
    - if(Accept cookis) suggerimenti list<Cocktail>
 
  if(Accept cookis){
    if(esperineza personalizzata){
      coctail miglioramento suggerimenti
    }
    ci salvimo tutti i tui dati e li utilizzio
  }
  else if(!Accept cookis){
    random basata sugli altri utenti
  }

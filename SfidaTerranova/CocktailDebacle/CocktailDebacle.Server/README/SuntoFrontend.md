# CocktailDebacle

## Avvio del Progetto
```bash

È possibile avviare il progetto dalla cartella `CocktailDebacle` con il comando:


docker-compose up --build

Modalità di sviluppo
Durante la fase di sviluppo, poiché i file di Angular sono caricati staticamente su un container Docker con Nginx, si consiglia di:

Avviare solo i servizi backend e database con: docker-compose up sqlserver backend

Avviare il frontend localmente con:


ng serve --host 0.0.0.0 --poll
🔥 In questa configurazione, i file Angular vengono serviti direttamente da locale, rendendo più semplice lo sviluppo.

Per far sì che l'app Angular si colleghi al database all'interno di Docker, è necessario modificare la stringa di connessione nel file:

backend/appsettings.json
Struttura del Frontend
Il progetto frontend è realizzato con Angular e utilizza componenti stand-alone, che comunicano tra loro tramite il file:

ts
app.routes.ts

Alcuni componenti, come il modale che mostra i cocktails, sono integrati all'interno di altri:


Backend Communication
Le chiamate API al backend (per utenti, cocktails, ecc.) sono gestite attraverso dei service, come ad esempio:


user.service.ts

Funzionalità Sign-Up
Nel form di registrazione sono utilizzati dei Validators per assicurarsi che i dati rispettino criteri specifici.

Traduzioni
Il sistema di traduzione è gestito tramite Azure Translation Services, con un limite di 2 milioni di caratteri.
È accessibile dalle impostazioni della Home, nella sezione inferiore, attraverso elementi del footer:

WhoAreWe

Privacy

Help

Accessibili:

Solo da loggati: nella pagina Profilo

Sempre accessibili: nella pagina Browse Cocktails


Pagina Browse Cocktails
La pagina permette di:

Cercare cocktails alcolici e non

Filtrare per ingredienti o persone

⚠️ Se l’utente non è loggato, può cercare solo cocktails analcolici.

Include 3 caroselli:

Mostrano suggerimenti personalizzati (se accettati i cookies)

Si adattano all’età dell’utente

Descrizioni dinamiche in base all’ora del giorno (mattina/pomeriggio/sera) e alla stagione


È possibile passare da modalità giorno/notte cliccando l'icona sole/luna.

Sidebar
Presente nella pagina Browse Cocktails e nella pagina Profilo, la sidebar permette di:

Cercare

Navigare tra:

Home

Browse

Profilo


Pagina Profilo
Include:

Utenti seguiti / follower

Numero di like ai cocktails

Cocktails creati (se pubblici)

Cocktails piaciuti


Per seguire un utente:

Imposta il filtro di ricerca su "Utenti"

Cerca il nome

Apri il profilo

Visualizza:

Cocktails creati

Possibilità di seguirlo/smettere di seguirlo

Funzionalità Profilo
Creazione nuovi cocktails

Visualizzazione cocktail piaciuti

Accesso alle Impostazioni

Qui si possono modificare:

Nome

Email

Username (unico)

Password (unica)

⚠️ Inserire uno username o password già esistenti genera errore.

Conclusione
Il progetto CocktailDebacle integra un frontend Angular modulare, un backend API in .NET e un database SQL Server, tutto orchestrato via Docker.
In fase di sviluppo, si consiglia di usare i container solo per backend e database, mantenendo il frontend su Angular CLI in locale per rapidità.

yaml
Copia
Modifica

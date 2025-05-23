# 🥂 CocktailDebacle - Progetto Full Stack con Docker, .NET e Angular

## 📁 Struttura del Progetto

- `CocktailDebacle.Server/`  
  Contiene il backend sviluppato in **ASP.NET Core** (.NET 8).  
  File principali:
  - `Program.cs`: entry point dell'app
  - `Controllers/`: API endpoints
  - `Models/`, `DTOs/`, `Migrations/`: modello dati e mapping
  - `Dockerfile`: istruzioni per il container del backend

- `cocktaildebacle.client/`  
  Contiene il frontend sviluppato in **Angular**.  
  File principali:
  - `src/`: codice sorgente Angular
  - `nginx/`: configurazione del reverse proxy
  - `Dockerfile`: build Angular + Nginx static hosting

- `init-db/`  
  Script SQL o inizializzazioni da eseguire all'avvio del container SQL Server.

- `docker-compose.yml`  
  File principale per orchestrare i container:
  - `sqlserver`: database SQL Server
  - `backend`: API ASP.NET
  - `frontend`: frontend Angular servito via Nginx

---

## 🐳 Comandi Docker
🔧 Build dei container
```bash
docker-compose build
```
▶️ Avvio dei container
```bash
docker-compose up
```
⛔️ Stop dei container
```bash
docker-compose down
```

📋 Elencare i container attivi
```bash
docker-compose ps
```

🔧 Eseguire un comando all’interno di un container
```bash
docker-compose exec <servizio> <comando>
```

🧠 Con Docker Compose v2+
Se usi Docker Compose v2 (in Docker Desktop o recente), il comando cambia:
``` bash
docker compose up -d     
```

# senza il trattino!
Entrambi funzionano se hai ancora docker-compose installato, ma docker compose è lo standard moderno.


## 🌐 Accesso

Frontend Angular: http://localhost:4200

Backend ASP.NET Core API: http://localhost:5052

SQL Server: Porta 1433 (puoi connetterti con SQL Server Management Studio o Azure Data Studio)

per far partire il programma spostarsi in ~CockatielBarV1.0\Main\SfidaTerranova\CocktailDebacle\

digitare docker-compose up --build
N.B (è necessario un servizio Docker, es. Docker-Desktop)
aspettare il completamento del compose--
connettersi a http://localhost:4200/


IN ALTERNATIVA (con i giusti servizi installati -- dotnet, angular, sqlserver etc. -- )

andare su ~CockatielBarV1.0\Main\SfidaTerranova\CocktailDebacle\CocktailDebacle.Server\ 
e avviare il back-end con dotnet watch run (è possibile che sia necessario prima eseguire migrazione e update del database -- è possibile farlo trami dotnet ef migrations add "NOME MIGRAZIONE" asepttare il completamento e digitare dotnet ef database update)

una volta che il servizio è in ascolto, dopo che si sarà aperto lo swagger,
andare nella cartella  ~CockatielBarV1.0\Main\SfidaTerranova\CocktailDebacle\cocktaildebacle.client\  e digitare
ng serve --host 0.0.0.0 --poll 
infine collegati a  http://localhost:4200/ 

N.B 
per permettere al programma di comunicare con il database andrà settata la stringa di connessione in appsettings.json in base all'ambiente di sviluppo
(es. Docker --> "DefaultConnection": "Server=sqlserver;Database=CocktailDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;" Locale(metti il tuo server) --> "Server=MTT\\SQLEXPRESS; Database=CocktailDebacle; Trusted_Connection=true; TrustServerCertificate=True; MultipleActiveResultSets=True";)

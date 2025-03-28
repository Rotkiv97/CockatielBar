# Dokerizare il server 
1) Creare un dockerfile nel WebApp.server
2) Creare un dockerfile nel WebApp.client
3) Creare un docker-compose.yml nell root del progetto
## Comando per creare il dockerfile
```powershell
New-Item -Path . -Name "Dockerfile" -ItemType "File"
```
## Comando per creare il dokcer-composer.yml
```powershell
New-Item -Path . -Name "docker-compose.yml" -ItemType "File"
```

### Vantaggi nell'uso dei docker
- Ambienti uniformi, si lavora con la stessa configurazione
- Facilità di setup tramite il docker-compose.yml
- E' possibile eseguire l'app in ogni ambinte

## TOODOO build sql Server .Net core e angular

###

1) struttura database
2) Dokerizzare il sever e il forntend
3) Home page
4) login (Recupero dati in caso di rimbambimento)
5) registazione login
6) logout

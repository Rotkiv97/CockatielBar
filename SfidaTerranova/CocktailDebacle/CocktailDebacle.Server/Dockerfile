# Usa l'SDK di .NET per la build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia il progetto e ripristina i pacchetti
COPY . ./
RUN dotnet restore

# Compila il progetto
RUN dotnet publish -c Release -o out

# Usa l'immagine runtime per eseguire l'app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Esponi la porta su cui gira il backend
EXPOSE 5000

# Comando di avvio del server
CMD ["dotnet", "Cocktail.Server.dll"]
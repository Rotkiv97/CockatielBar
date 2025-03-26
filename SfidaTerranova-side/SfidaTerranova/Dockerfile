# Usa l'immagine ufficiale di MySQL come base
FROM mysql:latest

# Imposta le variabili d'ambiente per la configurazione di MySQL
ENV MYSQL_ROOT_PASSWORD=rootpassword
ENV MYSQL_DATABASE=mydatabase
ENV MYSQL_USER=myuser
ENV MYSQL_PASSWORD=mypassword

# Copia uno script SQL personalizzato nel container
COPY init.sql /docker-entrypoint-initdb.d/

# Esponi la porta 3306 (la porta predefinita per MySQL)
EXPOSE 3306
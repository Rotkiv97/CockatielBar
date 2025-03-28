# Usa Node.js per la build
FROM node:20 AS build
WORKDIR /app

# Copia il package.json e installa le dipendenze
COPY package.json package-lock.json ./
RUN npm install

# Copia il resto del progetto
COPY . .

# Costruisci il progetto Angular
RUN npm run build

# Usa un server Nginx per servire i file statici
FROM nginx:alpine

# Copia la configurazione personalizzata di Nginx
COPY nginx/default.conf /etc/nginx/conf.d/default.conf

# Copia i file statici generati dalla build Angular
COPY --from=build /app/dist/cokatiels/browser /usr/share/nginx/html

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
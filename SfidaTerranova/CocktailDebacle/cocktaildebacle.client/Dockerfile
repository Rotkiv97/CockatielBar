# Usa Node.js per la build
FROM node:20 AS build
WORKDIR /app

# Copia il package.json e installa le dipendenze
COPY package.json package-lock.json ./
RUN npm install

# Copia il resto del progetto
COPY . .

# Costruisci il progetto Angular
RUN npm run build --prod

# Usa un server Nginx per servire i file statici
FROM nginx:alpine
COPY --from=build /app/dist/cokatiels /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
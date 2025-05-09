FROM node:20 AS build
WORKDIR /app

# 1. Copia SOLO i file necessari per l'installazione
COPY package.json package-lock.json ./ 

# 2. Esegui npm install una sola volta
RUN npm install --legacy-peer-deps --force

# 3. Copia tutto il resto del codice
COPY . .

# 4. Build con la tua configurazione originale
RUN npm run build

# Fase produzione (IDENTICA alla tua)
FROM nginx:alpine
COPY --from=build /app/dist/cokatiels/browser /usr/share/nginx/html
COPY nginx/default.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]

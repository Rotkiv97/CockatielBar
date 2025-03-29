FROM node:20 AS build
WORKDIR /app

# 1. Copia SOLO i file necessari per l'installazione
COPY package.json package-lock.json ./

# 2. Fix MIRACOLOSO (2 righe aggiuntive)
RUN npm install --legacy-peer-deps --force
RUN npm install @angular/animations@19.2.2 @angular/core@19.2.2 --no-audit --fund=false

# 3. Copia tutto il resto
COPY . .

# 4. Build con la tua configurazione originale
RUN npm run build

# Fase produzione (IDENTICA alla tua)
FROM nginx:alpine
COPY --from=build /app/dist/cokatiels/browser /usr/share/nginx/html
COPY nginx/default.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
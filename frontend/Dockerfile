# ---- build (multi-stage: a imagem final não carrega Node nem node_modules) ----
FROM node:22-alpine AS build
WORKDIR /app

# Copiar só os manifestos antes do código aproveita o cache: a instalação só refaz
# quando as dependências mudam, não a cada alteração de componente.
COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build

# ---- runtime ----
FROM nginx:alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
# O Angular publica os arquivos estáticos em browser/ dentro do diretório de saída.
COPY --from=build /app/dist/gestao-colaboradores-web/browser /usr/share/nginx/html
EXPOSE 80

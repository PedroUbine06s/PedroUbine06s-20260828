# ---- build (multi-stage: imagem final sem SDK) ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY GestaoColaboradores.sln ./
COPY src/GestaoColaboradores.Domain/*.csproj src/GestaoColaboradores.Domain/
COPY src/GestaoColaboradores.Application/*.csproj src/GestaoColaboradores.Application/
COPY src/GestaoColaboradores.Infrastructure/*.csproj src/GestaoColaboradores.Infrastructure/
COPY src/GestaoColaboradores.Api/*.csproj src/GestaoColaboradores.Api/
RUN dotnet restore src/GestaoColaboradores.Api/GestaoColaboradores.Api.csproj

COPY src/ src/
RUN dotnet publish src/GestaoColaboradores.Api/GestaoColaboradores.Api.csproj -c Release -o /app

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0

# libgssapi-krb5-2: o driver do PostgreSQL sonda a biblioteca GSSAPI/Kerberos na
# inicialização. O projeto autentica por usuário e senha, mas sem ela o Npgsql loga
# um "Error" na primeira linha.
#
# curl: a imagem de runtime do ASP.NET não traz wget nem curl, então o healthcheck
# do compose não conseguia sondar /health — falhava com "wget: not found" e deixava
# a API marcada como unhealthy mesmo tendo subido normalmente.
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "GestaoColaboradores.Api.dll"]

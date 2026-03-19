# -----------------------------
# Fase base (runtime)
# -----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

# Usuário padrão do Visual Studio (ou Dokploy)
USER $APP_UID
WORKDIR /app

# Portas que o app vai expor
EXPOSE 8080
EXPOSE 8081

# -----------------------------
# Fase build (compilação)
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia apenas o csproj correto
COPY BSERVE_API/BSERVE_API/BSERVE_API.csproj ./BSERVE_API/

# Restaura dependências (rápido, sem copiar todo o código ainda)
RUN dotnet restore "./BSERVE_API/BSERVE_API.csproj"

# Copia todo o restante do código
COPY . .

# Define o diretório do projeto para build
WORKDIR "/src/BSERVE_API"

# Build do projeto
RUN dotnet build "./BSERVE_API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# -----------------------------
# Fase publish (produção)
# -----------------------------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release

# Publica em Release (sem app host)
RUN dotnet publish "./BSERVE_API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# -----------------------------
# Fase final (imagem de produção)
# -----------------------------
FROM base AS final
WORKDIR /app

# Copia os arquivos publicados
COPY --from=publish /app/publish .

# EntryPoint da aplicação
ENTRYPOINT ["dotnet", "BSERVE_API.dll"]
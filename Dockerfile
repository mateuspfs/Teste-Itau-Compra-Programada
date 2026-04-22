# Etapa 1: Runtime de Base
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Etapa 2: SDK para Build e Publish
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar arquivos de projeto de forma otimizada para cache de camadas
COPY ["src/Itau.CompraProgramada.API/Itau.CompraProgramada.API.csproj", "src/Itau.CompraProgramada.API/"]
COPY ["src/Itau.CompraProgramada.Application/Itau.CompraProgramada.Application.csproj", "src/Itau.CompraProgramada.Application/"]
COPY ["src/Itau.CompraProgramada.Domain/Itau.CompraProgramada.Domain.csproj", "src/Itau.CompraProgramada.Domain/"]
COPY ["src/Itau.CompraProgramada.Infrastructure/Itau.CompraProgramada.Infrastructure.csproj", "src/Itau.CompraProgramada.Infrastructure/"]
COPY ["src/Itau.CompraProgramada.Worker/Itau.CompraProgramada.Worker.csproj", "src/Itau.CompraProgramada.Worker/"]

# Restaurar dependências
RUN dotnet restore "src/Itau.CompraProgramada.API/Itau.CompraProgramada.API.csproj"

# Copiar todo o código fonte
COPY . .

# Buildar o projeto de API
WORKDIR "/src/src/Itau.CompraProgramada.API"
RUN dotnet build "Itau.CompraProgramada.API.csproj" -c Release -o /app/build

# Etapa 3: Publicação dos binários
FROM build AS publish
RUN dotnet publish "Itau.CompraProgramada.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 4: Imagem Final de Produção (Leve e Segura)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Definir variáveis de ambiente padrão para produção
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Itau.CompraProgramada.API.dll"]

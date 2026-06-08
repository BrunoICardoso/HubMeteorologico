# Acesse https://aka.ms/customizecontainer para saber como personalizar seu contêiner de depuração e como o Visual Studio usa este Dockerfile para criar suas imagens para uma depuração mais rápida.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ENV TZ=America/Sao_Paulo
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Esta fase é usada para compilar o projeto de serviço
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["HubMeteorologico/HubMeteorologico.API.csproj", "HubMeteorologico/"]
COPY ["HubMeteorologico.Application/HubMeteorologico.Application.csproj", "HubMeteorologico.Application/"]
COPY ["HubMeteorologico.Domain/HubMeteorologico.Domain.csproj", "HubMeteorologico.Domain/"]
COPY ["HubMeteorologico.Infrastructure/HubMeteorologico.Infrastructure.csproj", "HubMeteorologico.Infrastructure/"]
RUN dotnet restore "./HubMeteorologico/HubMeteorologico.API.csproj"
COPY . .
WORKDIR "/src/HubMeteorologico"
RUN dotnet build "./HubMeteorologico.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase é usada para publicar o projeto de serviço a ser copiado para a fase final
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./HubMeteorologico.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta fase é usada na produção ou quando executada no VS no modo normal (padrão quando não está usando a configuração de Depuração)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HubMeteorologico.API.dll"]
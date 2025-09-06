# Dockerfile para EstudoApi.Banking
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["EstudoApi.Banking/EstudoApi.Banking.csproj", "EstudoApi.Banking/"]
COPY ["EstudoApi.Domain/EstudoApi.Domain.csproj", "EstudoApi.Domain/"]
COPY ["EstudoApi.Infrastructure/EstudoApi.Infrastructure.csproj", "EstudoApi.Infrastructure/"]
RUN dotnet restore "EstudoApi.Banking/EstudoApi.Banking.csproj"
COPY . .
WORKDIR "/src/EstudoApi.Banking"
RUN dotnet build "EstudoApi.Banking.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EstudoApi.Banking.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EstudoApi.Banking.dll"]

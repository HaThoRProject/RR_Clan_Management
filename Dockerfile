# Alap image .NET futtatókörnyezethez
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Build image .NET SDK-val
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RR_Clan_Management.csproj", "./"]
RUN dotnet restore "./RR_Clan_Management.csproj"

COPY . .
WORKDIR "/src"
RUN dotnet build "RR_Clan_Management.csproj" -c Release -o /app/build

# Publikálás
FROM build AS publish
RUN dotnet publish "RR_Clan_Management.csproj" -c Release -o /app/publish

# Végső image a futtatáshoz
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RR_Clan_Management.dll"]

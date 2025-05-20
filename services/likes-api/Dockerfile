# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY likes-api.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

# Etapa de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 8088
ENTRYPOINT ["dotnet", "likes-api.dll"]

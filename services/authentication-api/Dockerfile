FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY src/*.csproj ./src/

WORKDIR /app/src
RUN dotnet restore

WORKDIR /app
COPY . .

WORKDIR /app/src
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 8084

ENTRYPOINT ["dotnet", "src.dll"]

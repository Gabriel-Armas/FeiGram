FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY app/app.csproj ./app/
WORKDIR /src/app
RUN dotnet restore

COPY app/. ./
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8089

ENTRYPOINT ["dotnet", "app.dll"]
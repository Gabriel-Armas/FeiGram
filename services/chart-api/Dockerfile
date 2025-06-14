FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ChartApi/ChartApi.csproj ChartApi/
RUN dotnet restore ChartApi/ChartApi.csproj

COPY ChartApi/ ChartApi/
WORKDIR /src/ChartApi
RUN dotnet publish -c Release -o /app/publish

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8087
ENTRYPOINT ["dotnet", "ChartApi.dll"]

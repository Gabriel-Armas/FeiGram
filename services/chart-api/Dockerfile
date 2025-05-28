FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ChartApi/ChartApi.csproj ChartApi/
RUN dotnet restore ChartApi/ChartApi.csproj

COPY ChartApi/ ChartApi/
WORKDIR /src/ChartApi
RUN dotnet publish -c Release -o /app/publish

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Copia el certificado
COPY localhost.pfx .

# Expone los puertos que usará Kestrel según tu Program.cs
EXPOSE 6000
EXPOSE 6001

ENTRYPOINT ["dotnet", "ChartApi.dll"]

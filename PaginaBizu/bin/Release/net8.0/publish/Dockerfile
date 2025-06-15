# Etapa 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos todo el proyecto
COPY . .

# Publicamos la app en Release en la carpeta /app/publish
RUN dotnet publish -c Release -o /app/publish

# Etapa 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Copiamos los archivos publicados del build anterior
COPY --from=build /app/publish .

# Ejecutamos la app
ENTRYPOINT ["dotnet", "PaginaBizu.dll"]

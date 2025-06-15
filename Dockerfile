# Imagen base para ejecutar la app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Imagen de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos el contenido del proyecto
COPY . .

# Publicamos la aplicación (esto genera /src/bin/Release/net8.0/publish)
RUN dotnet publish -c Release -o /app/publish

# Imagen final
FROM base AS final
WORKDIR /app

# Ahora sí, copiamos los archivos publicados desde la ruta correcta
COPY --from=build /app/publish .

# Ejecutamos la app
ENTRYPOINT ["dotnet", "PaginaBizu.dll"]

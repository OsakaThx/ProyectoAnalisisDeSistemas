# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Stage base: runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# Exponer puerto donde Kestrel escucha (configurado por defecto en 80)
EXPOSE 80

# Stage build: SDK image para compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar el archivo de proyecto y restaurar dependencias
COPY ["PaginaBizu.csproj", "./"]
RUN dotnet restore "PaginaBizu.csproj"

# Copiar el resto del código y compilar
COPY . .
RUN dotnet build "PaginaBizu.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage publish: generar los artefactos de publicación
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PaginaBizu.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage final: crear la imagen de producción
FROM base AS final
# ?? Agrega aquí las dependencias nativas necesarias para SqlClient
RUN apt-get update \
 && apt-get install -y --no-install-recommends libkrb5-3 \
 && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "PaginaBizu.dll"]
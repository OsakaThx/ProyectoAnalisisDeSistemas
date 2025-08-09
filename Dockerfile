# Use the ASP.NET Core runtime as the base image for the final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the .NET SDK for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution file and restore dependencies
COPY ["PaginaBizu.sln", "./"]
COPY ["PaginaBizu/PaginaBizu.csproj", "PaginaBizu/"]
RUN dotnet restore "PaginaBizu/PaginaBizu.csproj"

# Copy the rest of the application code
COPY . .

# Build and publish the application
WORKDIR "/src/PaginaBizu"
RUN dotnet build "PaginaBizu.csproj" -c Release -o /app/build
RUN dotnet publish "PaginaBizu.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Create the final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Set the entry point for the container
ENTRYPOINT ["dotnet", "PaginaBizu.dll"]

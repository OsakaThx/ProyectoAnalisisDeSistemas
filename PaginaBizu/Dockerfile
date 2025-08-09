# Use the .NET SDK for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["PaginaBizu.sln", "./"]
COPY ["PaginaBizu/PaginaBizu.csproj", "PaginaBizu/"]

# Restore dependencies
RUN dotnet restore "PaginaBizu.sln"

# Copy the rest of the application code
COPY . .

# Build and publish the application
WORKDIR "/src/PaginaBizu"
RUN dotnet publish -c Release -o /app/publish

# Create the final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Create a directory for the database
RUN mkdir -p /app/data

# Copy the published application
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port 80
EXPOSE 80

# Set the entry point
ENTRYPOINT ["dotnet", "PaginaBizu.dll"]

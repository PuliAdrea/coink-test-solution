# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy NuGet.config first to override any global DevExpress fallback folder settings
COPY NuGet.config .

# Copy all source files (obj/bin excluded by .dockerignore to avoid cached assets with DevExpress references)
COPY . .

# Restore dependencies (using NuGet.config we just copied)
RUN dotnet restore UserManagement.sln

# Build and publish the API project
WORKDIR /src/UserManagement.API
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create logs directory
RUN mkdir -p /app/logs

# Copy published files from build stage
COPY --from=build /app/publish .

# Expose port (default HTTP port for .NET)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080

# Run the application
ENTRYPOINT ["dotnet", "UserManagement.API.dll"]

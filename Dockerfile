# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files for restore
COPY SecureVault.slnx .
COPY src/SecureVault.Web/SecureVault.Web.csproj src/SecureVault.Web/

# Restore dependencies
RUN dotnet restore src/SecureVault.Web/SecureVault.Web.csproj

# Copy everything and build
COPY src/ src/
RUN dotnet publish src/SecureVault.Web/SecureVault.Web.csproj -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Security: run as non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser
USER appuser

COPY --from=build /app/publish .

# Heroku assigns PORT dynamically
ENV ASPNETCORE_URLS=http://+:${PORT:-5000}
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "SecureVault.Web.dll"]

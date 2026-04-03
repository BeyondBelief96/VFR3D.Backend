# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY VFR3D.Domain/*.csproj ./VFR3D.Domain/
COPY VFR3D.Infrastructure/*.csproj ./VFR3D.Infrastructure/
COPY VFR3D.API/*.csproj ./VFR3D.API/
COPY VFR3D.Backend.sln .
RUN dotnet restore VFR3D.API/VFR3D.API.csproj

# Copy source and publish
COPY . .
RUN dotnet publish VFR3D.API/VFR3D.API.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Railway sets PORT env var
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}
EXPOSE 8080

ENTRYPOINT ["dotnet", "VFR3D.API.dll"]

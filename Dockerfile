# ---- frontend: bundle TypeScript modules with esbuild -------------------
FROM node:22-alpine AS frontend
WORKDIR /src
COPY src/Nadiano.Web/package.json src/Nadiano.Web/package-lock.json ./
RUN npm ci
COPY src/Nadiano.Web/tsconfig.json src/Nadiano.Web/build.mjs ./
COPY src/Nadiano.Web/wwwroot/js ./wwwroot/js
RUN npm run build

# ---- build: publish the .NET application ---------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
COPY --from=frontend /src/wwwroot/dist src/Nadiano.Web/wwwroot/dist
RUN dotnet restore Nadiano.slnx
RUN dotnet publish src/Nadiano.Web/Nadiano.Web.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    -p:SkipFrontendBuild=true

# ---- runtime: minimal, non-root -------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# curl is used only by the container HEALTHCHECK below.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

RUN groupadd --system nadiano \
    && useradd --system --gid nadiano --home-dir /app --no-create-home nadiano \
    && mkdir -p /data \
    && chown -R nadiano:nadiano /app /data

COPY --from=build --chown=nadiano:nadiano /app/publish .
COPY --chown=nadiano:nadiano content /app/content

ENV ASPNETCORE_URLS=http://+:8080
ENV Nadiano__DataPath=/data
ENV Nadiano__ContentPath=/app/content
ENV ASPNETCORE_ENVIRONMENT=Production

USER nadiano
EXPOSE 8080
VOLUME ["/data"]

HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD curl -fsS http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "Nadiano.Web.dll"]

FROM node:20-alpine AS frontend-build
WORKDIR /src/frontend

COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci --no-audit --no-fund

COPY frontend/ ./
ARG VITE_API_BASE_URL=/api
ENV VITE_API_BASE_URL=${VITE_API_BASE_URL}
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /src/backend

COPY backend/ConstructionPayment.sln ./
COPY backend/src/ConstructionPayment.Domain/ConstructionPayment.Domain.csproj src/ConstructionPayment.Domain/
COPY backend/src/ConstructionPayment.Application/ConstructionPayment.Application.csproj src/ConstructionPayment.Application/
COPY backend/src/ConstructionPayment.Infrastructure/ConstructionPayment.Infrastructure.csproj src/ConstructionPayment.Infrastructure/
COPY backend/src/ConstructionPayment.Api/ConstructionPayment.Api.csproj src/ConstructionPayment.Api/
RUN dotnet restore ConstructionPayment.sln --nologo

COPY backend/ ./
RUN dotnet publish src/ConstructionPayment.Api/ConstructionPayment.Api.csproj \
    -c Release \
    -o /app/publish \
    /p:ContinuousIntegrationBuild=true \
    /p:UseAppHost=false
RUN mkdir -p /app/publish/wwwroot
COPY --from=frontend-build /src/frontend/dist/ /app/publish/wwwroot/

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
ENV DOTNET_EnableDiagnostics=0

RUN mkdir -p /tmp/cpms-uploads
COPY --from=backend-build /app/publish ./

EXPOSE 10000
ENTRYPOINT ["sh", "-c", "dotnet ConstructionPayment.Api.dll --urls http://0.0.0.0:${PORT:-10000}"]

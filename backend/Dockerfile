FROM node:20-alpine AS frontend-build
WORKDIR /src/frontend

COPY frontend/package*.json ./
RUN npm ci

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
RUN dotnet restore ConstructionPayment.sln

COPY backend/ ./
RUN dotnet publish src/ConstructionPayment.Api/ConstructionPayment.Api.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false
RUN mkdir -p /app/publish/wwwroot
COPY --from=frontend-build /src/frontend/dist/ /app/publish/wwwroot/

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

RUN mkdir -p /tmp/cpms-uploads
COPY --from=backend-build /app/publish ./

EXPOSE 10000
ENTRYPOINT ["sh", "-c", "dotnet ConstructionPayment.Api.dll --urls http://0.0.0.0:${PORT:-10000}"]

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ConstructionPayment.sln ./
COPY src/ConstructionPayment.Domain/ConstructionPayment.Domain.csproj src/ConstructionPayment.Domain/
COPY src/ConstructionPayment.Application/ConstructionPayment.Application.csproj src/ConstructionPayment.Application/
COPY src/ConstructionPayment.Infrastructure/ConstructionPayment.Infrastructure.csproj src/ConstructionPayment.Infrastructure/
COPY src/ConstructionPayment.Api/ConstructionPayment.Api.csproj src/ConstructionPayment.Api/
RUN dotnet restore ConstructionPayment.sln

COPY . .
RUN dotnet publish src/ConstructionPayment.Api/ConstructionPayment.Api.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

RUN mkdir -p /tmp/cpms-uploads
COPY --from=build /app/publish ./

EXPOSE 10000
ENTRYPOINT ["sh", "-c", "dotnet ConstructionPayment.Api.dll --urls http://0.0.0.0:${PORT:-10000}"]

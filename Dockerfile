FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/StockBite.Api/StockBite.Api.csproj", "src/StockBite.Api/"]
COPY ["src/StockBite.Application/StockBite.Application.csproj", "src/StockBite.Application/"]
COPY ["src/StockBite.Domain/StockBite.Domain.csproj", "src/StockBite.Domain/"]
COPY ["src/StockBite.Infrastructure/StockBite.Infrastructure.csproj", "src/StockBite.Infrastructure/"]
RUN dotnet restore "src/StockBite.Api/StockBite.Api.csproj"
COPY . .
RUN dotnet publish "src/StockBite.Api/StockBite.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "StockBite.Api.dll"]

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

WORKDIR /service

COPY . .

RUN dotnet restore src/MigrationService && dotnet publish src/MigrationService -o /app/publish -c Release

FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

COPY --from=builder /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "MigrationService.dll"]
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY WizardRPG.slnx ./
COPY WizardRPG.Api/WizardRPG.Api.csproj WizardRPG.Api/
COPY WizardRPG.Tests.Unit/WizardRPG.Tests.Unit.csproj WizardRPG.Tests.Unit/
COPY WizardRPG.Tests.Integration/WizardRPG.Tests.Integration.csproj WizardRPG.Tests.Integration/

RUN dotnet restore WizardRPG.slnx

COPY . .

RUN dotnet publish WizardRPG.Api/WizardRPG.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WizardRPG.Api.dll"]

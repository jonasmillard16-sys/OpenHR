# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files first for caching
COPY Directory.Build.props Directory.Packages.props ./
COPY RegionHR.sln ./
COPY src/SharedKernel/RegionHR.SharedKernel.csproj src/SharedKernel/
COPY src/Infrastructure/RegionHR.Infrastructure.csproj src/Infrastructure/
COPY src/Api/RegionHR.Api.csproj src/Api/
COPY src/Modules/Core/RegionHR.Core.csproj src/Modules/Core/
COPY src/Modules/Payroll/RegionHR.Payroll.csproj src/Modules/Payroll/
COPY src/Modules/Scheduling/RegionHR.Scheduling.csproj src/Modules/Scheduling/
COPY src/Modules/CaseManagement/RegionHR.CaseManagement.csproj src/Modules/CaseManagement/
COPY src/Modules/LAS/RegionHR.LAS.csproj src/Modules/LAS/
COPY src/Modules/HalsoSAM/RegionHR.HalsoSAM.csproj src/Modules/HalsoSAM/
COPY src/Modules/SalaryReview/RegionHR.SalaryReview.csproj src/Modules/SalaryReview/
COPY src/Modules/Travel/RegionHR.Travel.csproj src/Modules/Travel/
COPY src/Modules/Recruitment/RegionHR.Recruitment.csproj src/Modules/Recruitment/
COPY src/Modules/IntegrationHub/RegionHR.IntegrationHub.csproj src/Modules/IntegrationHub/
COPY src/Modules/SelfService/RegionHR.SelfService.csproj src/Modules/SelfService/
COPY src/DesignSystem/RegionHR.DesignSystem.csproj src/DesignSystem/

# Restore
RUN dotnet restore src/Api/RegionHR.Api.csproj

# Copy everything and build
COPY src/ src/
RUN dotnet publish src/Api/RegionHR.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Non-root user for security
RUN groupadd -r regionhr && useradd -r -g regionhr regionhr
USER regionhr

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "RegionHR.Api.dll"]

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and props for restore caching
COPY Directory.Build.props Directory.Packages.props ./
COPY RegionHR.sln ./

# Copy all csproj files for restore caching
COPY src/SharedKernel/RegionHR.SharedKernel.csproj src/SharedKernel/
COPY src/Infrastructure/RegionHR.Infrastructure.csproj src/Infrastructure/
COPY src/Api/RegionHR.Api.csproj src/Api/
COPY src/Web/RegionHR.Web.csproj src/Web/
COPY src/DesignSystem/RegionHR.DesignSystem.csproj src/DesignSystem/
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
COPY src/Modules/Audit/RegionHR.Audit.csproj src/Modules/Audit/
COPY src/Modules/Notifications/RegionHR.Notifications.csproj src/Modules/Notifications/
COPY src/Modules/Leave/RegionHR.Leave.csproj src/Modules/Leave/
COPY src/Modules/Documents/RegionHR.Documents.csproj src/Modules/Documents/
COPY src/Modules/Performance/RegionHR.Performance.csproj src/Modules/Performance/
COPY src/Modules/Reporting/RegionHR.Reporting.csproj src/Modules/Reporting/
COPY src/Modules/GDPR/RegionHR.GDPR.csproj src/Modules/GDPR/
COPY src/Modules/Competence/RegionHR.Competence.csproj src/Modules/Competence/
COPY src/Modules/Benefits/RegionHR.Benefits.csproj src/Modules/Benefits/
COPY src/Modules/LMS/RegionHR.LMS.csproj src/Modules/LMS/
COPY src/Modules/Positions/RegionHR.Positions.csproj src/Modules/Positions/
COPY src/Modules/Offboarding/RegionHR.Offboarding.csproj src/Modules/Offboarding/
COPY src/Modules/Analytics/RegionHR.Analytics.csproj src/Modules/Analytics/
COPY src/Modules/Configuration/RegionHR.Configuration.csproj src/Modules/Configuration/

# Restore
RUN dotnet restore RegionHR.sln

# Copy everything and build
COPY src/ src/
RUN dotnet publish src/Web/RegionHR.Web.csproj -c Release -o /app/publish --no-restore

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

ENTRYPOINT ["dotnet", "RegionHR.Web.dll"]

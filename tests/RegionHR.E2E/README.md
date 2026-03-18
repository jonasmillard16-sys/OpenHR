# E2E Tests

## Setup
```bash
dotnet new nunit -n RegionHR.E2E -o tests/RegionHR.E2E
dotnet add tests/RegionHR.E2E package Microsoft.Playwright.NUnit
npx playwright install
```

## Run
```bash
dotnet test tests/RegionHR.E2E
```

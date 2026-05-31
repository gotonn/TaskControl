$ErrorActionPreference = "Stop"

if (-not (Test-Path ".config/dotnet-tools.json")) {
    dotnet new tool-manifest
}

dotnet tool restore

if (-not (Test-Path "Migrations") -or -not (Get-ChildItem "Migrations" -Filter "*InitialCreate*.cs" -ErrorAction SilentlyContinue)) {
    dotnet ef migrations add InitialCreate
}

dotnet ef database update
Write-Host "Database is ready. Run: dotnet run" -ForegroundColor Green

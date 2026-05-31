#!/usr/bin/env bash
set -e

if [ ! -f ".config/dotnet-tools.json" ]; then
  dotnet new tool-manifest
fi

dotnet tool restore

if [ ! -d "Migrations" ] || ! ls Migrations/*InitialCreate*.cs >/dev/null 2>&1; then
  dotnet ef migrations add InitialCreate
fi

dotnet ef database update
echo "Database is ready. Run: dotnet run"

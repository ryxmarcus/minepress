#!/usr/bin/env bash
set -e

# Run EF migrations, then start the app
# Assumes the published app and dotnet runtime are present in the image

if [ -n "$DOTNET_USE_PG_MIGRATION" ]; then
  echo "Running EF Core migrations..."
  # Try to run migrations using dotnet-ef if available
  if command -v dotnet-ef >/dev/null 2>&1; then
    dotnet ef database update --no-build
  else
    echo "dotnet-ef not installed in image; skipping migrations."
  fi
fi

# Start the app
exec dotnet erp.minepress.web.dll

#!/bin/sh
set -e

# Initialize database if the volume is empty
if [ ! -f /app/data/cargohub.db ]; then
    echo "Initializing CargoHub database..."
    cp /app/cargohub.db /app/data/cargohub.db
fi

echo "Starting CargoHub API..."

exec dotnet CargoHub_refact.dll

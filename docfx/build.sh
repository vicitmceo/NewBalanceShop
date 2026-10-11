#!/usr/bin/env bash
# Генерує сайт документації коду в docs/code (DocFX) і прибирає source maps, щоб зменшити розмір.
# Запуск (WSL або Linux): ./docfx/build.sh
# Потрібен DocFX:  dotnet tool install docfx --tool-path ~/docfx
set -e
cd "$(dirname "$0")"
DOCFX="${DOCFX:-docfx}"
"$DOCFX" docfx.json
find ../docs/code -name '*.map' -delete

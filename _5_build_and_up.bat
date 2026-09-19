@echo off
echo Building and up...
docker compose ^
  -f compose.yml ^
  -f compose.app.yml ^
  -f compose.observability.yml up --build
echo Done.
pause
@echo off
echo Building...
docker compose ^
  -f compose.yml ^
  -f compose.app.yml ^
  -f compose.observability.yml build
echo Done.
pause
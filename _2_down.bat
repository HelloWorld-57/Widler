@echo off
echo Stopping and removing containers...
docker compose ^
  -f compose.yml ^
  -f compose.app.yml ^
  -f compose.observability.yml down
echo Done.
pause
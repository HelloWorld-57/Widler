@echo off
echo Stopping and removing containers + volumes...
docker compose ^
  -f compose.yml ^
  -f compose.app.yml ^
  -f compose.observability.yml down --volumes
echo Done.
pause


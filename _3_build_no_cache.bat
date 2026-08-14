@echo off
echo Building without cache...
docker compose ^
  -f compose.yml ^
  -f compose.app.yml ^
  -f compose.observability.yml build --no-cache
echo Done.
pause
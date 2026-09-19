@echo off
echo Up...
docker compose ^
  -f compose.yml ^
  -f compose.app.yml ^
  -f compose.observability.yml start
echo Done.
pause
@echo off
echo Stopping and removing containers + volumes...
docker compose down --volumes
echo Done.
pause
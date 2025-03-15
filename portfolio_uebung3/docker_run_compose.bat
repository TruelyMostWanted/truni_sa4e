@echo off
cd /d %~dp0
docker-compose -f compose.yaml up
pause
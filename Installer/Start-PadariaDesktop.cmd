@echo off
setlocal
set "APP_DIR=%~dp0"
set "SERVER=%APP_DIR%server\InfiniteCoffee2.exe"
set "DESKTOP=%APP_DIR%desktop\infinite_coffee_app.exe"

if not exist "%SERVER%" (
  echo Backend nao encontrado em "%SERVER%".
  pause
  exit /b 1
)
if not exist "%DESKTOP%" (
  echo Aplicativo desktop nao encontrado em "%DESKTOP%".
  pause
  exit /b 1
)

start "Padaria Debortolo - API local" /min "%SERVER%" --urls "http://0.0.0.0:5049"
timeout /t 3 /nobreak >nul
start "Padaria Debortolo" "%DESKTOP%"
endlocal

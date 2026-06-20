@echo off
cd /d "%~dp0"
echo Starting AVA Security Server...
echo --------------------------------
echo Checking Node version:
node --version
echo.
echo Installing dependencies (if missing)...
call npm install
echo.
echo Starting Server...
npm run dev
pause

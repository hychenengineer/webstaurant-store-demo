@echo off
setlocal enabledelayedexpansion

echo =======================================================
echo   Building WebstaurantStore Mini-IDS Standalone EXE
echo =======================================================

set ROOT_DIR=%~dp0

:: 1. Build React Frontend
echo.
echo [1/4] Building React 19 Frontend with Vite...
cd /d "%ROOT_DIR%frontend"
call npm run build
if %errorlevel% neq 0 (
    echo [ERROR] Frontend build failed.
    pause
    exit /b %errorlevel%
)

:: 2. Copy compiled UI to backend wwwroot
echo.
echo [2/4] Syncing frontend build to backend\wwwroot...
cd /d "%ROOT_DIR%"
if exist "%ROOT_DIR%backend\wwwroot" rmdir /s /q "%ROOT_DIR%backend\wwwroot"
xcopy /e /i /y "%ROOT_DIR%frontend\dist" "%ROOT_DIR%backend\wwwroot" >nul

:: 3. Publish self-contained single-file executable
echo.
echo [3/4] Publishing self-contained single-file Windows executable...
cd /d "%ROOT_DIR%backend"
dotnet publish Webstaurant.IDS.Api.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "%ROOT_DIR%publish"
if %errorlevel% neq 0 (
    echo [ERROR] dotnet publish failed.
    pause
    exit /b %errorlevel%
)

:: Add a quick launcher script inside the publish folder
echo @echo off > "%ROOT_DIR%publish\start.cmd"
echo title WebstaurantStore Mini-IDS >> "%ROOT_DIR%publish\start.cmd"
echo start "" "%%~dp0WebstaurantStore-Mini-IDS.exe" >> "%ROOT_DIR%publish\start.cmd"

:: 4. Compress to Distribution Zip
echo.
echo [4/4] Creating distribution ZIP package...
cd /d "%ROOT_DIR%"
powershell -NoProfile -Command "Compress-Archive -Path 'publish\*' -DestinationPath 'WebstaurantStore-Mini-IDS-Windows-x64.zip' -Force"

echo.
echo =======================================================
echo   Build Successful!
echo   - Executable: publish\WebstaurantStore-Mini-IDS.exe
echo   - Package:    WebstaurantStore-Mini-IDS-Windows-x64.zip
echo =======================================================
echo.
pause

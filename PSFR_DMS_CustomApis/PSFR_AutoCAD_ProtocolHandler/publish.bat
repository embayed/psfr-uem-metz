@echo off
setlocal EnableDelayedExpansion

echo ============================================
echo Building Intalio AutoCAD Protocol Handler
echo ============================================
echo.

echo [1/2] Building application...
dotnet publish -c Release --output ".\bin\Release\publish"

if !ERRORLEVEL! NEQ 0 (
    echo.
    echo ============================================
    echo Build failed! Check the errors above.
    echo ============================================
    pause
    exit /b 1
)

echo.
echo ============================================
echo Build succeeded!
echo ============================================
echo.

echo [2/2] Creating installer with Inno Setup...
echo.

REM Check if Inno Setup is installed
set INNO_FOUND=0

if exist "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" (
    set INNO_FOUND=1
    set "ISCC=C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
)

if exist "C:\Program Files\Inno Setup 6\ISCC.exe" (
    set INNO_FOUND=1
    set "ISCC=C:\Program Files\Inno Setup 6\ISCC.exe"
)

if !INNO_FOUND! EQU 0 (
    echo WARNING: Inno Setup not found!
    echo.
    echo Please download and install Inno Setup from:
    echo https://jrsoftware.org/isdl.php
    echo.
    echo Standalone executable is available at:
    echo .\bin\Release\publish\PSFR_AutoCAD_ProtocolHandler.exe
    echo.
    explorer ".\bin\Release\publish"
    goto :END
)

echo Found Inno Setup
echo Compiling installer...
echo.

"!ISCC!" "Setup.iss"

if !ERRORLEVEL! EQU 0 (
    echo.
    echo ============================================
    echo Installer created successfully!
    echo ============================================
    echo.
    echo Setup file location:
    echo .\Setup\Intalio_AutoCAD_ProtocolHandler_Setup.exe
    echo.
    echo You can now distribute this setup file to clients!
    echo.
    explorer ".\Setup"
) else (
    echo.
    echo ============================================
    echo Installer creation failed!
    echo ============================================
    echo.
    echo Standalone executable is still available at:
    echo .\bin\Release\publish\PSFR_AutoCAD_ProtocolHandler.exe
    echo.
)

:END
pause

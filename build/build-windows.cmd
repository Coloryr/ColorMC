@echo off
setlocal enabledelayedexpansion

set "version="
set "main_version="

for /f %%i in ('type .\build\version') do (
    set "version=%%i"
)

for /f %%i in ('type .\build\main_version') do (
    set "main_version=%%i"
)

mkdir .\build_out

call :build_win win-x64
call :build_win win-x86
call :build_win win-arm64
call :build_win_aot win-x64
call :build_win_aot win-arm64

goto :eof

:build_win
echo build colormc-%main_version%%version%-%1.zip

dotnet publish .\src\ColorMC.Launcher -p:PublishProfile=%1

mkdir .\src\build_out\%1-dotnet\colormc

set "files=ColorMC.Gui.pdb ColorMC.Core.pdb Live2DCSharpSDK.App.pdb Live2DCSharpSDK.Framework.pdb ColorMC.Launcher.pdb ColorMC.Launcher.exe"

for %%f in (%files%) do (
    copy .\src\build_out\%1-dotnet\%%f .\src\build_out\%1-dotnet\colormc\%%f
)

@REM set "zip_name=colormc-a%version%-%1.zip"

@REM cd .\src\build_out\%1-dotnet\
@REM powershell Compress-Archive -Path .\colormc -DestinationPath ..\..\..\build_out\%zip_name% -Force

@REM cd ..\..\..\

echo colormc-%main_version%%version%-%1.zip build done
goto :eof

:build_win_aot
echo build colormc-%main_version%%version%-%1-aot.zip

dotnet publish .\src\ColorMC.Launcher -p:PublishProfile=%1-aot

mkdir .\src\build_out\%1-aot\colormc

set "files=ColorMC.Launcher.exe av_libglesv2.dll libHarfBuzzSharp.dll libSkiaSharp.dll SDL2.dll"

for %%f in (%files%) do (
    copy .\src\build_out\%1-aot\%%f .\src\build_out\%1-aot\colormc\%%f
)

@REM set "zip_name=colormc-a%version%-%1-aot.zip"

@REM cd .\src\build_out\%1-aot\
@REM powershell Compress-Archive -Path .\colormc -DestinationPath ..\..\..\build_out\%zip_name% -Force

@REM cd ..\..\..\

echo colormc-%main_version%%version%-%1-aot.zip build done
goto :eof
@echo off
setlocal enabledelayedexpansion

set "version="

for /f %%i in ('type .\build\version') do (
    set "version=%%i"
)

mkdir .\build_out

call :build_win win-x64
call :build_win win-x86
call :build_win win-arm64
call :build_win_min win-x64
call :build_win_min win-arm64

goto :eof

:build_win
echo build colormc-win-%version%-%1.exe

dotnet publish .\src\ColorMC.Launcher -p:PublishProfile=%1

mkdir .\build_out\%1-dotnet

copy .\src\build_out\%1-dotnet\ColorMC.Launcher.exe .\build_out\%1-dotnet\colormc-win-%version%-%1.exe

echo colormc-win-%version%-%1.exe build done
goto :eof

:build_win_min
echo build colormc-win-%version%-min-%1.exe

dotnet publish .\src\ColorMC.Launcher -p:PublishProfile=%1-min

mkdir .\build_out\%1-min

copy .\src\build_out\%1-min\ColorMC.Launcher.exe .\build_out\%1-min\colormc-win-%version%-min-%1.exe

echo colormc-win-%version%-min-%1.exe build done
goto :eof
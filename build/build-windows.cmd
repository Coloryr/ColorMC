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

call :build_win win-x64 x86_64
call :build_win win-x86 x86
call :build_win win-arm64 aarch64
call :build_win_min win-x64 x86_64
call :build_win_min win-arm64 aarch64
call :build_win_aot win-x64 x86_64
call :build_win_aot win-arm64 aarch64

goto :eof

:build_win
echo build colormc-win-%main_version%%version%-%2.zip

dotnet publish .\src\ColorMC.Launcher -p:PublishProfile=%1

mkdir .\src\build_out\%1-dotnet\colormc

set "files=av_libglesv2.dll ColorMC.Core.pdb ColorMC.Gui.pdb ColorMC.Launcher.exe ColorMC.Launcher.pdb libHarfBuzzSharp.dll libSkiaSharp.dll Live2DCSharpSDK.App.pdb Live2DCSharpSDK.Framework.pdb Live2DCSharpSDK.OpenGL.pdb MinecraftSkinRender.OpenGL.pdb MinecraftSkinRender.pdb SDL2.dll X11.pdb"

for %%f in (%files%) do (
    copy .\src\build_out\%1-dotnet\%%f .\src\build_out\%1-dotnet\colormc\%%f
)

echo colormc-win-%main_version%%version%-%2.zip build done
goto :eof

:build_win_aot
echo build colormc-win-%main_version%%version%-aot-%2.zip

dotnet publish .\src\ColorMC.Launcher -p:PublishProfile=%1-aot

mkdir .\src\build_out\%1-aot\colormc

set "files=ColorMC.Launcher.exe av_libglesv2.dll libHarfBuzzSharp.dll libSkiaSharp.dll SDL2.dll"

for %%f in (%files%) do (
    copy .\src\build_out\%1-aot\%%f .\src\build_out\%1-aot\colormc\%%f
)

echo colormc-win-%main_version%%version%-aot-%2.zip build done
goto :eof

:build_win_min
echo build colormc-win-%main_version%%version%-min-%2.zip

dotnet publish .\src\ColorMC.Launcher -p:PublishProfile=%1-min

mkdir .\src\build_out\%1-min\colormc

set "files=av_libglesv2.dll ColorMC.Core.pdb ColorMC.Gui.pdb ColorMC.Launcher.exe ColorMC.Launcher.pdb libHarfBuzzSharp.dll libSkiaSharp.dll Live2DCSharpSDK.App.pdb Live2DCSharpSDK.Framework.pdb Live2DCSharpSDK.OpenGL.pdb MinecraftSkinRender.OpenGL.pdb MinecraftSkinRender.pdb SDL2.dll X11.pdb"

for %%f in (%files%) do (
    copy .\src\build_out\%1-min\%%f .\src\build_out\%1-min\colormc\%%f
)

echo colormc-win-%main_version%%version%-min-%2.zip build done
goto :eof
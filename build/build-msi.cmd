choco install wixtoolset
dotnet tool install --global wix --version 4.0.4
wix extension add -g WixToolset.UI.wixext/4.0.4

for /f %%i in ('type .\build\version') do (
    set "version=%%i"
)

for /f %%i in ('type .\build\main_version') do (
    set "main_version=%%i"
)

cd .\src\ColorMC.Setup.Wix
dotnet build
COPY colormc-x64.msi colormc-windows-%main_version%%version%-x64.msi 
COPY colormc-x64-aot.msi colormc-windows-%main_version%%version%-aot-x64.msi 
COPY colormc-x64-min.msi colormc-windows-%main_version%%version%-min-x64.msi 
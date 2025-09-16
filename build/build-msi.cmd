choco install wixtoolset
dotnet tool install --global wix --version 4.0.4
wix extension add -g WixToolset.UI.wixext/4.0.4

for /f %%i in ('type .\build\version') do (
    set "version=%%i"
)

cd .\src\ColorMC.Setup.Wix
dotnet build
COPY colormc-x64.msi colormc-windows-%version%-x64.msi 
COPY colormc-x64-aot.msi colormc-windows-%version%-aot-x64.msi 
COPY colormc-x64-min.msi colormc-windows-%version%-min-x64.msi 
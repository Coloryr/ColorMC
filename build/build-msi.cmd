choco install wixtoolset
dotnet tool install --global wix --version 4.0.4
wix extension add -g WixToolset.UI.wixext/4.0.4

cd .\src\ColorMC.Setup.Wix
dotnet build
COPY colormc-x64.msi colormc-windows-${{ env.MAINVERSION }}${{ env.VERSION }}-x64.msi 
COPY colormc-x64-aot.msi colormc-windows-${{ env.MAINVERSION }}${{ env.VERSION }}-aot-x64.msi 
COPY colormc-x64-min.msi colormc-windows-${{ env.MAINVERSION }}${{ env.VERSION }}-min-x64.msi 
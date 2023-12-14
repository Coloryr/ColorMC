#!/bin/bash

version=""

for line in `cat ./build/version`
do
    version=$line
done

mkdir ./build_out

echo "ColorMC build win64 version: $version"

dotnet publish ./src/ColorMC.Launcher -p:PublishProfile=Win-x64

mkdir ./src/build_out/win64-dotnet/colormc

pdbs=("ColorMC.Gui.pdb" "ColorMC.Core.pdb" "Live2DCSharpSDK.App.pdb"
    "Live2DCSharpSDK.Framework.pdb" ColorMC.Launcher.pdb "ColorMC.Launcher.exe")

for line in ${pdbs[@]}
do
    mv ./src/build_out/win64-dotnet/$line \
        ./src/build_out/win64-dotnet/colormc/$line
done

zip_name="colormc-a$version-win64.zip"

cd ./src/build_out/win64-dotnet/
zip -r $zip_name ./colormc
mv $zip_name ../../../build_out/$zip_name

echo "ColorMC win64 build done"
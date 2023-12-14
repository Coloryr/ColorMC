#!/bin/bash

version=""

for line in `cat ./build/version`
do
    version=$line
done

echo "ColorMC build win64 version: $version"

# sudo apt-get install p7zip-full -y

rm -rf ./src/build_out/win64-dotnet

dotnet publish ./src/ColorMC.Launcher -p:PublishProfile=Win-x64

rm colormc-a$version-win64.7z

mkdir ./src/build_out/win64-dotnet/colormc

pdbs=("ColorMC.Gui.pdb" "ColorMC.Core.pdb" "Live2DCSharpSDK.App.pdb"
    "Live2DCSharpSDK.Framework.pdb" ColorMC.Launcher.pdb "ColorMC.Launcher.exe")

for line in ${pdbs[@]}
do
    mv ./src/build_out/win64-dotnet/$line \
        ./src/build_out/win64-dotnet/colormc/$line
done

7z a colormc-a$version-win64.7z ./src/build_out/win64-dotnet/colormc/

echo "ColorMC win64 build done"
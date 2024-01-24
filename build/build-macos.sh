#!/bin/bash

version=""

for line in `cat ./build/version`
do
    version=$line
done

echo "ColorMC build osx-x64 version: $version"

mkdir ./build_out

base=./src/build_out/osx-x64-dotnet
base_dir="$base/ColorMC.app/Contents"

rm $zip_name
rm -rf $base

dotnet publish ./src/ColorMC.Launcher -p:PublishProfile=osx-x64

mkdir $base/ColorMC.app
mkdir $base_dir

pdbs=("ColorMC.Gui.pdb" "ColorMC.Core.pdb" "Live2DCSharpSDK.App.pdb"
    "Live2DCSharpSDK.Framework.pdb" "ColorMC.Launcher.pdb" "ColorMC.Launcher"
    "libAvaloniaNative.dylib" "libHarfBuzzSharp.dylib" "libSkiaSharp.dylib")

cp -r ./build/info/osx64/* $base_dir

dir=MacOS

mkdir $base_dir/$dir

for line in ${pdbs[@]}
do
    cp $base/$line \
        $base_dir/$dir/$line
done

chmod a+x $base_dir/$dir/ColorMC.Launcher

zip_name="colormc-a$version-osx-x64.zip"

cd ./src/build_out/osx-x64-dotnet
zip -r $zip_name ./ColorMC.app
mv $zip_name ../../../build_out/$zip_name

echo "ColorMC osx-x64 build done"
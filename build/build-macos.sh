#!/bin/bash

version=""

for line in `cat ./build/version`
do
    version=$line
done

build_osx ()
{
    echo "ColorMC build $1 version: $version"

    mkdir ./build_out

    base=./src/build_out/$1-dotnet
    base_dir="$base/ColorMC.app/Contents"
    zip_name="colormc-a$version-$1.zip"

    dotnet publish ./src/ColorMC.Launcher -p:PublishProfile=$1

    mkdir $base/ColorMC.app
    mkdir $base_dir

    files=("ColorMC.Gui.pdb" "ColorMC.Core.pdb" "Live2DCSharpSDK.App.pdb"
        "Live2DCSharpSDK.Framework.pdb" "ColorMC.Launcher.pdb" "ColorMC.Launcher"
        "libAvaloniaNative.dylib" "libHarfBuzzSharp.dylib" "libSkiaSharp.dylib")

    cp -r ./build/info/$1/* $base_dir

    dir=$base_dir/MacOS

    mkdir $dir

    for line in ${files[@]}
    do
        cp $base/$line \
            $dir/$line
    done

    chmod a+x $dir/ColorMC.Launcher

    cd ./src/build_out/$1-dotnet
    zip -r $zip_name ./ColorMC.app
    mv $zip_name ../../../build_out/$zip_name

    echo "ColorMC $1 build done"
}

build_osx osx-x64
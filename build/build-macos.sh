#!/bin/bash

version=""
main_version=""

for line in `cat ./build/version`
do
    version=$line
done

for line in `cat ./build/main_version`
do
    main_version=$line
done

build_osx()
{
    echo "build colormc-$main_version$version-$1-macos.zip"

    mkdir ./build_out

    base=./src/build_out/$1-dotnet
    base_dir="$base/ColorMC.app/Contents"
    zip_name="colormc-$main_version$version-$1.zip"

    dotnet publish ./src/ColorMC.Launcher -p:PublishProfile=$1

    mkdir $base/ColorMC.app
    mkdir $base_dir

    files=("ColorMC.Gui.pdb" "ColorMC.Core.pdb" "Live2DCSharpSDK.App.pdb"
        "Live2DCSharpSDK.Framework.pdb" "ColorMC.Launcher.pdb" "ColorMC.Launcher"
        "libAvaloniaNative.dylib" "libHarfBuzzSharp.dylib" "libSkiaSharp.dylib"
        "libSDL2-2.0.dylib" "X11.pdb")

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

    echo "colormc-$main_version$version-$1-macos.zip build done"
}

build_osx osx-x64
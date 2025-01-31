#!/bin/bash

version=""
main_version=""
replace_sdl="false"

for line in `cat ./build/version`
do
    version=$line
done

for line in `cat ./build/main_version`
do
    main_version=$line
done

build_sdl_arm64()
{
    git clone https://github.com/libsdl-org/SDL.git
    cd SDL
    mkdir build
    cd build
    ../configure
    make -j8
    cd ../../
}

build_osx()
{    
    zip_name="colormc-macos-$main_version$version-$2.zip"

    echo "build $zip_name"

    mkdir ./build_out

    base=./src/build_out/$1-dotnet
    base_dir="$base/ColorMC.app/Contents"

    dotnet publish ./src/ColorMC.Launcher -p:PublishProfile=$1

    mkdir $base/ColorMC.app
    mkdir $base_dir

    files=("ColorMC.Gui.pdb" "ColorMC.Core.pdb" "ColorMC.Launcher" "ColorMC.Launcher.pdb" 
        "libAvaloniaNative.dylib" "libHarfBuzzSharp.dylib" "libSkiaSharp.dylib"
        "libSDL2-2.0.dylib" "Live2DCSharpSDK.App.pdb" "Live2DCSharpSDK.Framework.pdb"
        "Live2DCSharpSDK.OpenGL.pdb" "MinecraftSkinRender.OpenGL.pdb" "MinecraftSkinRender.pdb" "X11.pdb")

    cp -r ./build/info/osx/* $base_dir

    dir=$base_dir/MacOS

    mkdir $dir

    for line in ${files[@]}
    do
        cp $base/$line \
            $dir/$line
    done

    chmod a+x $dir/ColorMC.Launcher

    if [$1 = 'aarch64'] && [replace_sdl = 'true'] ;then
        build_sdl_arm64
    fi

    cd ./src/build_out/$1-dotnet

    if [$1 = 'aarch64'] && [replace_sdl = 'true'] ;then
        rm libSDL2-2.0.dylib
        cp ../../../SDL/build/build/.libs/libSDL2-2.0.0.dylib libSDL2-2.0.dylib
    fi

    codesign --force --deep --sign - ColorMC.app
    zip -r $zip_name ./ColorMC.app
    mv $zip_name ../../../build_out/$zip_name
    cd ../../../

    echo "$zip_name build done"
}

build_osx_min()
{
    zip_name="colormc-macos-$main_version$version-min-$2.zip"

    echo "build $zip_name"

    mkdir ./build_out

    base=./src/build_out/$1-min
    base_dir="$base/ColorMC.app/Contents"

    dotnet publish ./src/ColorMC.Launcher -p:PublishProfile=$1-min

    mkdir $base/ColorMC.app
    mkdir $base_dir

    files=("ColorMC.Gui.pdb" "ColorMC.Core.pdb" "ColorMC.Launcher" "ColorMC.Launcher.pdb" 
        "libAvaloniaNative.dylib" "libHarfBuzzSharp.dylib" "libSkiaSharp.dylib"
        "libSDL2-2.0.dylib" "Live2DCSharpSDK.App.pdb" "Live2DCSharpSDK.Framework.pdb"
        "Live2DCSharpSDK.OpenGL.pdb" "MinecraftSkinRender.OpenGL.pdb" "MinecraftSkinRender.pdb" "X11.pdb")

    cp -r ./build/info/osx/* $base_dir

    dir=$base_dir/MacOS

    mkdir $dir

    for line in ${files[@]}
    do
        cp $base/$line \
            $dir/$line
    done

    chmod a+x $dir/ColorMC.Launcher

    if [$1 = 'aarch64'] && [replace_sdl = 'true'] ;then
        build_sdl_arm64
    fi

    cd ./src/build_out/$1-min

    if [$1 = 'aarch64'] && [replace_sdl = 'true'] ;then
        rm libSDL2-2.0.dylib
        cp ../../../SDL/build/build/.libs/libSDL2-2.0.0.dylib libSDL2-2.0.dylib
    fi

    zip -r $zip_name ./ColorMC.app
    mv $zip_name ../../../build_out/$zip_name
    cd ../../../

    echo "$zip_name build done"
}

build_osx osx-x64 x86_64
build_osx osx-arm64 aarch64
build_osx_min osx-x64 x86_64
build_osx_min osx-arm64 aarch64
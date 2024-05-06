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

build_arch() 
{
    echo "build colormc-$main_version$version-$1.pkg.tar.zst"

    base=./src/build_out/$1-dotnet
    base_dir="$base/colormc_arch"

    mkdir $base_dir

    pdbs=("ColorMC.Gui.pdb" "ColorMC.Core.pdb" "Live2DCSharpSDK.App.pdb"
        "Live2DCSharpSDK.Framework.pdb" "ColorMC.Launcher.pdb" "X11.pdb"
        "libHarfBuzzSharp.so" "libSDL2-2.0.so" "libSkiaSharp.so" "ColorMC.Launcher")

    for line in ${pdbs[@]}
    do
        cp $base/$line \
            $base_dir/$line
    done

    info=./build/info/linux/usr/share

    cp ./build/info/arch/PKGBUILD $base_dir/PKGBUILD
    cp ./build/info/arch/colormc.install $base_dir/colormc.install
    cp $info/applications/ColorMC.desktop $base_dir/ColorMC.desktop
    cp $info/icons/colormc.png $base_dir/colormc.png

    sed -i "s/%version%/$version/g" $base_dir/PKGBUILD
    sed -i "s/%arch%/$2/g" $base_dir/PKGBUILD

    cd $base_dir
    makepkg -f

    cd ../../../../

    cp $base_dir/colormc-$version-1-$2.pkg.tar.zst \
        ./build_out/colormc-$main_version$version-1-$2.pkg.tar.zst

    echo "colormc-$main_version$version-$1.pkg.tar.zst build done"
}

build_arch_aot() 
{
    echo "build colormc-$main_version$version-$1-aot.pkg.tar.zst"

    base=./src/build_out/$1-aot
    base_dir="$base/colormc_arch"

    mkdir $base_dir

    pdbs=("libHarfBuzzSharp.so" "libSDL2-2.0.so" "libSkiaSharp.so" "ColorMC.Launcher")

    for line in ${pdbs[@]}
    do
        cp $base/$line \
            $base_dir/$line
    done

    info=./build/info/linux/usr/share

    cp ./build/info/arch/PKGBUILD-AOT $base_dir/PKGBUILD
    cp ./build/info/arch/colormc.install $base_dir/colormc.install
    cp $info/applications/ColorMC.desktop $base_dir/ColorMC.desktop
    cp $info/icons/colormc.png $base_dir/colormc.png

    sed -i "s/%version%/$version/g" $base_dir/PKGBUILD
    sed -i "s/%arch%/$2/g" $base_dir/PKGBUILD

    cd $base_dir
    makepkg -f

    cd ../../../../

    cp $base_dir/colormc-$version-1-$2.pkg.tar.zst \
        ./build_out/colormc-$main_version$version-1-$2-aot.pkg.tar.zst

    echo "colormc-$main_version$version-$1-aot.pkg.tar.zst build done"
}

build_arch linux-x64 x86_64
# build_arch linux-arm64 aarch64
build_arch_aot linux-x64 x86_64
# build_arch_aot linux-arm64 aarch64

#!/bin/bash

build_arch() {
    echo "build ColorMC-$1.pkg.tar.zst version: $version"

    base_dir=./src/build_out/$1-dotnet
    info=./build/info/linux/usr/share

    cp ./build/info/arch/PKGBUILD $base_dir/PKGBUILD
    cp ./build/info/arch/install $base_dir/.INSTALL
    cp $info/applications/ColorMC.desktop $base_dir/ColorMC.desktop
    cp $info/icons/colormc.png $base_dir/colormc.png

    sed -i "s/%version%/$version/g" $base_dir/PKGBUILD
    sed -i "s/%arch%/$2/g" $base_dir/PKGBUILD

    cd ./src/build_out/$1-dotnet
    makepkg -f

    cd ../../../

    mv $base_dir/colormc-a$version-1-$2.pkg.tar.zst \
        ./build_out/colormc-a$version-1-$2.pkg.tar.zst

    echo "ColorMC-$1.pkg.tar.zst build done"
}

build_arch_aot() {
    echo "build ColorMC-$1-aot.pkg.tar.zst version: $version"

    base_dir=./src/build_out/$1-aot
    info=./build/info/linux/usr/share

    cp ./build/info/arch/PKGBUILD-AOT $base_dir/PKGBUILD
    cp ./build/info/arch/install $base_dir/.INSTALL
    cp $info/applications/ColorMC.desktop $base_dir/ColorMC.desktop
    cp $info/icons/colormc.png $base_dir/colormc.png

    sed -i "s/%version%/$version/g" $base_dir/PKGBUILD
    sed -i "s/%arch%/$2/g" $base_dir/PKGBUILD

    cd ./src/build_out/$1-aot
    makepkg -f

    cd ../../../

    mv $base_dir/colormc-a$version-1-$2.pkg.tar.zst \
        ./build_out/colormc-a$version-1-$2-aot.pkg.tar.zst

    echo "ColorMC-$1-aot.pkg.tar.zst build done"
}

build_arch linux-x64 x86_64
build_arch linux-arm64 aarch64
build_arch_aot linux-x64 x86_64
build_arch_aot linux-arm64 aarch64
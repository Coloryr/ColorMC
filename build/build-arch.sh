#!/bin/bash

version=""

for line in `cat ./build/version`
do
    version=$line
done

mkdir ./build_out

build_arch() {
    echo "ColorMC build arch $1 version: $version"

    base_dir=./src/build_out/$1-dotnet
    info=./build/info/linux/usr/share

    rm -rf $base_dir

    dotnet publish ./src/ColorMC.Launcher -p:PublishProfile=$1

    cp ./build/info/arch/PKGBUILD $base_dir/PKGBUILD
    cp ./build/info/arch/install $base_dir/.INSTALL
    cp $info/applications/ColorMC.desktop $base_dir/ColorMC.desktop
    cp $info/icons/colormc.png $base_dir/colormc.png

    sed -i "s/%version%/$version/g" $base_dir/PKGBUILD

    cd ./src/build_out/$1-dotnet
    makepkg -f

    cd ../../../

    mv $base_dir/colormc-a$version-$version-x86_64.pkg.tar.zst \
        ./build_out/colormc-a$version-$version-x86_64.pkg.tar.zst

    echo "ColorMC arch $1 build done"
}

build_arch_aot() {
    echo "ColorMC build arch $1-aot version: $version"

    base_dir=./src/build_out/$1-aot
    info=./build/info/linux/usr/share

    rm -rf $base_dir

    dotnet publish ./src/ColorMC.Launcher -p:PublishProfile=$1-aot

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

    echo "ColorMC arch $1-aot build done"
}

build_arch linux-x64 x86_64
build_arch linux-arm64 aarch64
build_arch_aot linux-x64 x86_64
build_arch_aot linux-arm64 aarch64
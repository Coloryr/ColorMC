#!/bin/bash

version=""

for line in `cat ./build/version`
do
    version=$line
done

build_arch() {
    echo "ColorMC build arch linux-$2 version: $version"

    base_dir=./src/build_out/linux-$2-dotnet
    info=./build/info/linux/usr/share

    rm -rf $base_dir

    dotnet publish ./src/ColorMC.Launcher -p:PublishProfile=$1

    cp ./build/info/arch/PKGBUILD $base_dir/PKGBUILD
    cp ./build/info/arch/install $base_dir/.INSTALL
    cp $info/applications/ColorMC.desktop $base_dir/ColorMC.desktop
    cp $info/icons/colormc.png $base_dir/colormc.png

    sed -i "s/%version%/$version/g" $base_dir/PKGBUILD

    cd ./src/build_out/linux-$2-dotnet
    makepkg -f

    cd ../../../

    mv $base_dir/colormc-a$version-$version-x86_64.pkg.tar.zst \
        colormc-a$version-$version-x86_64.pkg.tar.zst

    echo "ColorMC arch linux-$2 build done"
}

build_arch Linux-x64 amd64


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

    mv $base_dir/colormc-$version-1-$2.pkg.tar.zst \
        ./build_out/colormc-$main_version$version-1-$2.pkg.tar.zst

    echo "colormc-$main_version$version-$1.pkg.tar.zst build done"
}

build_arch_aot() 
{
    echo "build colormc-$main_version$version-$1-aot.pkg.tar.zst"

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

    mv $base_dir/colormc-$version-1-$2.pkg.tar.zst \
        ./build_out/colormc-$main_version$version-1-$2-aot.pkg.tar.zst

    echo "colormc-$main_version$version-$1-aot.pkg.tar.zst build done"
}

build_arch linux-x64 x86_64
# build_arch linux-arm64 aarch64
build_arch_aot linux-x64 x86_64
# build_arch_aot linux-arm64 aarch64
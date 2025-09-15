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
    pkg=colormc-linux-$main_version$version-1-$2.pkg.tar.zst

    echo "build $pkg"

    base=./src/build_out/$1-dotnet
    base_dir="$base/colormc_arch"

    mkdir $base_dir

    pdbs=("ColorMC.Launcher" "libHarfBuzzSharp.so" "libSDL2-2.0.so" "libSkiaSharp.so")

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
    cp $info/mime/packages/colormc.xml $base_dir/colormc.xml

    sed -i "s/%version%/$version/g" $base_dir/PKGBUILD
    sed -i "s/%arch%/$2/g" $base_dir/PKGBUILD

    cd $base_dir
    makepkg -f

    cd ../../../../

    cp $base_dir/colormc-$version-1-$2.pkg.tar.zst ./build_out/$pkg

    echo "$pkg build done"
}

build_arch_aot() 
{
    pkg=colormc-linux-$main_version$version-1-aot-$2.pkg.tar.zst

    echo "build $pkg"

    base=./src/build_out/$1-aot
    base_dir="$base/colormc_arch"

    mkdir $base_dir

    pdbs=("ColorMC.Launcher" "libHarfBuzzSharp.so" "libSDL2-2.0.so" "libSkiaSharp.so")

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
    cp $info/mime/packages/colormc.xml $base_dir/colormc.xml

    sed -i "s/%version%/$version/g" $base_dir/PKGBUILD
    sed -i "s/%arch%/$2/g" $base_dir/PKGBUILD

    cd $base_dir
    makepkg -f

    cd ../../../../

    cp $base_dir/colormc-$version-1-$2.pkg.tar.zst ./build_out/$pkg

    echo "$$pkg build done"
}

build_arch_min() 
{
    pkg=colormc-linux-$main_version$version-1-min-$2.pkg.tar.zst

    echo "build $pkg"

    base=./src/build_out/$1-min
    base_dir="$base/colormc_arch"

    mkdir $base_dir

    pdbs=("ColorMC.Launcher" "libHarfBuzzSharp.so" "libSDL2-2.0.so" "libSkiaSharp.so")

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
    cp $info/mime/packages/colormc.xml $base_dir/colormc.xml

    sed -i "s/%version%/$version/g" $base_dir/PKGBUILD
    sed -i "s/%arch%/$2/g" $base_dir/PKGBUILD

    cd $base_dir
    makepkg -f

    cd ../../../../

    cp $base_dir/colormc-$version-1-$2.pkg.tar.zst ./build_out/$pkg

    echo "$pkg build done"
}

build_arch linux-x64 x86_64
# build_arch linux-arm64 aarch64
build_arch_aot linux-x64 x86_64
# build_arch_aot linux-arm64 aarch64
build_arch_min linux-x64 x86_64
# build_arch_min linux-arm64 aarch64

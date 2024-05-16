#!/bin/bash

sudo apt-get install rpm -y

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

build_rpm()
{
    echo "build colormc-$main_version$version-$1.rpm"

    base=./src/build_out/$1-dotnet
    base_dir="$base/colormc_rpm"

    mkdir -p $base_dir/{BUILD,RPMS,SOURCES,SPECS,SRPMS,BUILDROOT}

    pdbs=("ColorMC.Gui.pdb" "ColorMC.Core.pdb" "Live2DCSharpSDK.App.pdb"
        "Live2DCSharpSDK.Framework.pdb" "ColorMC.Launcher.pdb" "X11.pdb"
        "libHarfBuzzSharp.so" "libSDL2-2.0.so" "libSkiaSharp.so" "ColorMC.Launcher")

    bindir=$base_dir/BUILDROOT/colormc-$version-1.$2/usr/share

    mkdir -p $bindir
    mkdir -p $bindir/colormc
    mkdir -p $bindir/applications
    mkdir -p $bindir/icons

    info=./build/info/linux/usr/share

    cp ./build/info/rpm/build.spec $base_dir/SPECS
    cp $info/applications/ColorMC.desktop $bindir/applications/ColorMC.desktop
    cp $info/icons/colormc.png $bindir/icons/colormc.png

    sed -i "s/%version%/$version/g" $base_dir/SPECS/build.spec

    for line in ${pdbs[@]}
    do
        cp $base/$line \
            $bindir/colormc/$line
    done

    rpmbuild -bb --target=$2 $base_dir/SPECS/build.spec --define "_topdir %{getenv:PWD}/src/build_out/$1-dotnet/colormc_rpm"

    cp $base_dir/RPMS/$2/colormc-$version-1.$2.rpm ./build_out/colormc-$main_version$version-$2.rpm

    echo "build colormc-$main_version$version-$1.rpm done"
}

build_rpm_aot()
{
    echo "build colormc-$main_version$version-$1-aot.rpm"

    base=./src/build_out/$1-aot
    base_dir="$base/colormc_rpm"

    mkdir -p $base_dir/{BUILD,RPMS,SOURCES,SPECS,SRPMS,BUILDROOT}

    pdbs=("libHarfBuzzSharp.so" "libSDL2-2.0.so" "libSkiaSharp.so" "ColorMC.Launcher")

    bindir=$base_dir/BUILDROOT/colormc-$version-1.$2/usr/share

    mkdir -p $bindir
    mkdir -p $bindir/colormc
    mkdir -p $bindir/applications
    mkdir -p $bindir/icons

    info=./build/info/linux/usr/share

    cp ./build/info/rpm/build.spec $base_dir/SPECS
    cp $info/applications/ColorMC.desktop $bindir/applications/ColorMC.desktop
    cp $info/icons/colormc.png $bindir/icons/colormc.png

    sed -i "s/%version%/$version/g" $base_dir/SPECS/build.spec

    for line in ${pdbs[@]}
    do
        cp $base/$line \
            $bindir/colormc/$line
    done

    rpmbuild -bb --target=$2 $base_dir/SPECS/build.spec --define "_topdir %{getenv:PWD}/src/build_out/$1-dotnet/colormc_rpm"

    cp $base_dir/RPMS/$2/colormc-$version-1.$2.rpm ./build_out/colormc-$main_version$version-$2-aot.rpm

    echo "build colormc-$main_version$version-$1-aot.rpm done"
}

build_rpm linux-x64 x86_64
build_rpm linux-arm64 aarch64
build_rpm_aot linux-x64 x86_64
build_rpm_aot linux-arm64 aarch64
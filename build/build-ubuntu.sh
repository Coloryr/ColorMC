#!/bin/bash

version=""

for line in `cat ./build/version`
do
    version=$line
done

# $1 is Linux-x64
# $2 is amd64

build_arch() {
    echo "ColorMC build linux-$2 version: $version"

    base=./src/build_out/linux-$2-dotnet
    base_dir="$base/colormc"
    deb_name="./build_out/colormc-a$version-linux-$2.deb"

    rm -rf $base

    dotnet publish ./src/ColorMC.Launcher -p:PublishProfile=$1

    rm $deb_name

    mkdir $base_dir

    pdbs=("ColorMC.Gui.pdb" "ColorMC.Core.pdb" "Live2DCSharpSDK.App.pdb"
        "Live2DCSharpSDK.Framework.pdb" "ColorMC.Launcher.pdb" "ColorMC.Launcher")

    cp -r ./build/info/linux/* $base_dir
    cp -r ./build/info/linux-$2/* $base_dir

    sed -i "s/%version%/$version/g" $base_dir/DEBIAN/control

    dir=usr/share/ColorMC

    mkdir $base_dir/$dir

    for line in ${pdbs[@]}
    do
        cp $base/$line \
            $base_dir/$dir/$line
    done

    chmod -R 775 $base_dir/DEBIAN/postinst

    dpkg -b $base_dir $deb_name

    echo "ColorMC linux-$2 build done"
}

build_arch Linux-x64 amd64
build_arch Linux-ARM64 arm64

echo "ColorMC build linux-$2-appimage version: $version"

build_run=./build_run

mkdir $build_run

if [ ! -f "$build_run/deb2appimage.AppImage" ];then
    wget https://github.com/simoniz0r/deb2appimage/releases/download/v0.0.5/deb2appimage-0.0.5-x86_64.AppImage
    mv ./deb2appimage-0.0.5-x86_64.AppImage $build_run/deb2appimage.AppImage
fi

cp ./build/info/appimg.json $build_run/appimg.json

arch=amd64

sed -i "s/%version%/$version/g" $build_run/appimg.json
sed -i "s/%arch%/$arch/g" $build_run/appimg.json

chmod a+x $build_run/deb2appimage.AppImage

sudo .$build_run/deb2appimage.AppImage -j $build_run/appimg.json -o ./

sudo chown : colormc-a23-x86_64.AppImage

chmod a+x colormc-a$version-x86_64.AppImage

echo "ColorMC linux-appimage build done"
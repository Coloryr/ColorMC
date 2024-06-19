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

build_deb() 
{
    echo "build colormc-$main_version$version-$1.deb"

    base=./src/build_out/$1-dotnet
    base_dir="$base/colormc_deb"
    deb_name="./build_out/colormc-$main_version$version-$1.deb"

    mkdir $base_dir

    pdbs=("ColorMC.Gui.pdb" "ColorMC.Core.pdb" "ColorMC.Launcher" "ColorMC.Launcher.pdb" 
        "libHarfBuzzSharp.so" "libSDL2-2.0.so" "libSkiaSharp.so"
        "Live2DCSharpSDK.App.pdb" "Live2DCSharpSDK.Framework.pdb"  "X11.pdb")

    cp -r ./build/info/linux/* $base_dir
    cp -r ./build/info/$1/* $base_dir

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

    echo "colormc-$main_version$version-linux-$1.deb build done"
}

build_deb_aot() {

    echo "build colormc-$main_version$version-linux-$1-aot.deb"

    base=./src/build_out/$1-aot
    base_dir="$base/colormc_deb"
    deb_name="./build_out/colormc-$main_version$version-$1-aot.deb"

    mkdir $base_dir

    files=("libHarfBuzzSharp.so" "libSDL2-2.0.so" "libSkiaSharp.so" "ColorMC.Launcher")

    cp -r ./build/info/linux/* $base_dir
    cp -r ./build/info/$1/* $base_dir

    sed -i "s/%version%/$version/g" $base_dir/DEBIAN/control

    dir=usr/share/ColorMC

    mkdir $base_dir/$dir

    for line in ${files[@]}
    do
        cp $base/$line \
            $base_dir/$dir/$line
    done

    chmod -R 775 $base_dir/DEBIAN/postinst

    dpkg -b $base_dir $deb_name

    echo "colormc-$main_version$version-linux-$1-aot.deb build done"
}

build_deb linux-x64
build_deb linux-arm64
build_deb_aot linux-x64
build_deb_aot linux-arm64

echo "build colormc-$main_version$version-linux-x86_64.AppImage"

build_run=./build_run

mkdir $build_run

if [ ! -f "$build_run/deb2appimage.AppImage" ];then
    wget https://github.com/simoniz0r/deb2appimage/releases/download/v0.0.5/deb2appimage-0.0.5-x86_64.AppImage
    mv ./deb2appimage-0.0.5-x86_64.AppImage $build_run/deb2appimage.AppImage
fi

cp ./build/info/appimg.json $build_run/appimg.json

file_name=linux-x64
arch=amd64
deb_name=colormc-$main_version$version-linux-x64.deb

sed -i "s/%version%/$main_version$version/g" $build_run/appimg.json
sed -i "s/%arch%/$arch/g" $build_run/appimg.json
sed -i "s/%deb_name%/$deb_name/g" $build_run/appimg.json

chmod a+x $build_run/deb2appimage.AppImage

sudo apt-get install libfuse2 curl -y

sudo $build_run/deb2appimage.AppImage -j $build_run/appimg.json -o ./build_out

sudo chown $USER:$USER ./build_out/colormc-$main_version$version-x86_64.AppImage
chmod a+x build_out/colormc-$main_version$version-x86_64.AppImage
mv build_out/colormc-$main_version$version-x86_64.AppImage build_out/colormc-$main_version$version-linux-x86_64.AppImage

echo "ColorMC-$main_version$version-linux-x64-appimage build done"

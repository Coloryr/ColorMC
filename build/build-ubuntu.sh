#!/bin/bash
set -e

version=""

for line in `cat ./build/version`
do
    version=$line
done

build_deb() 
{
    deb=colormc-linux-$version-$2.deb

    echo "build $deb"

    base=./src/build_out/$1-dotnet
    base_dir="$base/colormc_deb"

    mkdir $base_dir

    pdbs=("ColorMC.Launcher" "libHarfBuzzSharp.so" "libSDL2-2.0.so" "libSkiaSharp.so")

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

    dpkg -b $base_dir ./build_out/$deb

    echo "$deb build done"
}

build_deb_min() 
{
    deb=colormc-linux-$version-min-$2.deb

    echo "build $deb"

    base=./src/build_out/$1-min
    base_dir="$base/colormc_deb"

    mkdir $base_dir

    pdbs=("ColorMC.Launcher" "libHarfBuzzSharp.so" "libSDL2-2.0.so" "libSkiaSharp.so")

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

    dpkg -b $base_dir ./build_out/$deb

    echo "$deb build done"
}

build_deb linux-x64 amd64
build_deb linux-arm64 arm64
build_deb_min linux-x64 amd64
build_deb_min linux-arm64 arm64

build_run=./build_run

mkdir $build_run

if [ ! -f "$build_run/deb2appimage.AppImage" ];then
    wget https://github.com/simoniz0r/deb2appimage/releases/download/v0.0.5/deb2appimage-0.0.5-x86_64.AppImage
    mv ./deb2appimage-0.0.5-x86_64.AppImage $build_run/deb2appimage.AppImage
fi

chmod a+x $build_run/deb2appimage.AppImage

if [ ! -f "$build_run/appimagetool.AppImage" ];then
    wget -O $build_run/appimagetool.AppImage https://github.com/AppImage/appimagetool/releases/download/continuous/appimagetool-x86_64.AppImage
fi

chmod a+x $build_run/appimagetool.AppImage

sudo apt-get install libfuse2 curl -y

build_appimage()
{
    appimg=colormc-$version-$2.AppImage
    
    build_dir=$build_run/$1
    
    mkdir $build_dir

    echo "build $appimg"

    cp ./build/info/appimg.json $build_dir/appimg.json

    arch=amd64
    deb_name=colormc-linux-$version-$1.deb

    sed -i "s/%version%/$version/g" $build_dir/appimg.json
    sed -i "s/%arch%/$arch/g" $build_dir/appimg.json
    sed -i "s/%deb_name%/$deb_name/g" $build_dir/appimg.json

    sudo $build_run/deb2appimage.AppImage -j $build_dir/appimg.json -o ./build_out

    sudo chown $USER:$USER ./build_out/colormc-$version-$2.AppImage
    chmod a+x build_out/colormc-$version-$2.AppImage
    #deb2appimage输出名固定为colormc-$version-$2.AppImage,与目标不同名时才重命名
    if [ "build_out/colormc-$version-$2.AppImage" != "build_out/$appimg" ]; then
        mv build_out/colormc-$version-$2.AppImage build_out/$appimg
    fi

    #用新版appimagetool重打包,新的runtime不再依赖libfuse2
    cd ./build_out
    ./$appimg --appimage-extract >/dev/null
    cd - >/dev/null
    $build_run/appimagetool.AppImage --no-appstream ./build_out/squashfs-root ./build_out/$appimg
    chmod a+x build_out/$appimg
    rm -rf ./build_out/squashfs-root

    echo "$appimg build done"
}

build_appimage_min()
{
    appimg=colormc-$version-min-$2.AppImage
    
    build_dir=$build_run/$1-min

    mkdir $build_dir

    echo "build $appimg"

    cp ./build/info/appimg.json $build_dir/appimg.json

    arch=amd64
    deb_name=colormc-linux-$version-min-$1.deb

    sed -i "s/%version%/$version/g" $build_dir/appimg.json
    sed -i "s/%arch%/$arch/g" $build_dir/appimg.json
    sed -i "s/%deb_name%/$deb_name/g" $build_dir/appimg.json

    sudo $build_run/deb2appimage.AppImage -j $build_dir/appimg.json -o ./build_out

    sudo chown $USER:$USER ./build_out/colormc-$version-$2.AppImage
    chmod a+x build_out/colormc-$version-$2.AppImage
    #deb2appimage输出名固定为colormc-$version-$2.AppImage,与目标不同名时才重命名
    if [ "build_out/colormc-$version-$2.AppImage" != "build_out/$appimg" ]; then
        mv build_out/colormc-$version-$2.AppImage build_out/$appimg
    fi

    #用新版appimagetool重打包,新的runtime不再依赖libfuse2
    cd ./build_out
    ./$appimg --appimage-extract >/dev/null
    cd - >/dev/null
    $build_run/appimagetool.AppImage --no-appstream ./build_out/squashfs-root ./build_out/$appimg
    chmod a+x build_out/$appimg
    rm -rf ./build_out/squashfs-root

    echo "$appimg build done"
}

build_appimage amd64 x86_64
build_appimage_min amd64 x86_64

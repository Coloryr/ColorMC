#!/bin/bash

version=""

for line in `cat ./build/version`
do
    version=$line
done

build_arch() {
    echo "ColorMC build linux-$2 version: $version"

    base=./src/build_out/linux-$2-dotnet
    base_dir="$base/colormc"
    deb_name="colormc-a$version-linux-$2.deb"

    rm -rf $base

    dotnet publish ./src/ColorMC.Launcher -p:PublishProfile=$1

    rm $deb_name

    mkdir $base_dir

    pdbs=("ColorMC.Gui.pdb" "ColorMC.Core.pdb" "Live2DCSharpSDK.App.pdb"
        "Live2DCSharpSDK.Framework.pdb" ColorMC.Launcher.pdb ColorMC.Launcher)

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

if [ ! -f "./build/debtap" ];then
    wget https://github.com/helixarch/debtap/archive/refs/tags/3.5.1.zip
    mv "3.5.1.zip" ./build/debtap.zip
    unzip ./build/debtap.zip -d ./build/debtap_pack
    mv ./build/debtap_pack/debtap-3.5.1/debtap ./build/debtap
    rm -rf ./build/debtap_pack/
    rm ./build/debtap.zip
    chmod a+x ./build/debtap
fi

sudo ./build/debtap -u
./build/debtap colormc-a23-linux-amd64.deb
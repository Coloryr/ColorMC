#!/usr/bin/env bash

VER=32
REL=1
URL='https://github.com/Coloryr/ColorMC/releases/download/a32.2024.11.4-2/colormc-linux-a32-1-x86_64.pkg.tar.zst'
TARGET='colormc-linux-a32-1-x86_64.pkg.tar.zst'
POSTURL=$(echo $URL | sed 's/\//\\\//g')
wget $URL -O $TARGET
SUM=$(sha256sum $TARGET | cut -f1 -d' ')

rm -rf out
mkdir out
cp PKGBUILD_Pre out/PKGBUILD
cp colormc.install out/colormc.install

cd out

sed -i "s/%ver%/$VER/g" PKGBUILD
sed -i "s/%rel%/$REL/g" PKGBUILD
sed -i "s/%source%/$POSTURL/g" PKGBUILD
sed -i "s/%sha%/$SUM/g" PKGBUILD
sed -i "s/%file%/$TARGET/g" PKGBUILD

makepkg --printsrcinfo > .SRCINFO
cd ../

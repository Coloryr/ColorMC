#!/usr/bin/env bash

VERSION=31
URL='https://github.com/Coloryr/ColorMC/releases/download/a31.2024.10.10/colormc-linux-a31-1-x86_64.pkg.tar.zst'
TARGET='colormc-linux-a31-1-x86_64.pkg.tar.zst'
POSTURL=$(echo $URL | sed 's/\//\\\//g')
wget $URL -O $TARGET
SUM=$(sha256sum $TARGET | cut -f1 -d' ')

sed -i 's/^source=(.*)/source=('"'"$POSTURL"'"')/' PKGBUILD
sed -i 's/^sha256sums=(.*)/sha256sums=('"'"$SUM"'"')/' PKGBUILD
sed -i 's/^pkgver=.*/pkgver='$VERSION'/' PKGBUILD

makepkg --printsrcinfo > .SRCINFO

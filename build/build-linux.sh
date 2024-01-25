#!/bin/bash

version=""

for line in `cat ./build/version`
do
    version=$line
done

mkdir ./build_out

build_linux() {

    echo "build ColorMC-$1 version: $version"

    dotnet publish ./src/ColorMC.Launcher -p:PublishProfile=$1

    echo "ColorMC $1 build done"
}

build_linux_aot() {

    echo "build ColorMC-$1-aot version: $version"

    dotnet publish ./src/ColorMC.Launcher -p:PublishProfile=$1-aot

    echo "ColorMC-$1-aot build done"
}

build_linux linux-x64
build_linux linux-arm64
build_linux_aot linux-x64
build_linux_aot linux-arm64
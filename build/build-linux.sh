#!/bin/bash

version=""

for line in `cat ./build/version`
do
    version=$line
done

mkdir ./build_out

build_linux() 
{
    echo "build colormc-$version-$1-linux"

    dotnet publish ./src/ColorMC.Launcher -p:PublishProfile=$1

    echo "colormc-$version-$1-linux build done"
}

build_linux_aot() 
{
    echo "build colormc-$version-$1-aot version: $version"

    dotnet publish ./src/ColorMC.Launcher -p:PublishProfile=$1-aot

    echo "colormc-$version-$1-aot build done"
}

build_linux_min() 
{
    echo "build colormc-$version-$1-min version: $version"

    dotnet publish ./src/ColorMC.Launcher -p:PublishProfile=$1-min

    echo "colormc-$version-$1-min build done"
}

build_linux linux-x64
build_linux linux-arm64
build_linux_min linux-x64
build_linux_min linux-arm64
build_linux_aot linux-x64
# build_linux_aot linux-arm64
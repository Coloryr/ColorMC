#!/bin/bash

version=""

for line in `cat ./build/version`
do
    version=$line
done

mkdir ./build_out

build_win() {

	echo "ColorMC build $1 version: $version"

	dotnet publish ./src/ColorMC.Launcher -p:PublishProfile=$1

	mkdir ./src/build_out/$1-dotnet/colormc

	files=("ColorMC.Gui.pdb" "ColorMC.Core.pdb" "Live2DCSharpSDK.App.pdb"
	    "Live2DCSharpSDK.Framework.pdb" ColorMC.Launcher.pdb "ColorMC.Launcher.exe")

	for line in ${files[@]}
	do
	    mv ./src/build_out/$1-dotnet/$line \
		./src/build_out/$1-dotnet/colormc/$line
	done

	zip_name="colormc-a$version-$1.zip"

	cd ./src/build_out/$1-dotnet/
	zip -r $zip_name ./colormc
	mv $zip_name ../../../build_out/$zip_name

	cd ../../../

	echo "ColorMC $1 build done"
}

build_win win-x64
build_win win-arm64
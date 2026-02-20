#!/bin/bash

CREATE_DMG_SCRIPT=""

version=""
for line in `cat ./build/version`
do
    version=$line
done

# 下载 create-dmg 脚本
download_create_dmg() {

    if [ -f "create-dmg" ]; then
        echo "create-dmg is downloaded"
        return 0
    fi
    
    echo "download create-dmg ..."
    
    if command -v git &> /dev/null; then
        git clone "https://github.com/create-dmg/create-dmg"
    else
        echo "git not found"
        return 1
    fi
    
    CREATE_DMG_SCRIPT="./create-dmg/create-dmg"
    chmod +x "$CREATE_DMG_SCRIPT"
}

build() {
    local app_name="ColorMC"
    local app_path="./src/build_out/$1-dotnet/ColorMC.app"
    local background=${2:-""}  # 可选背景图
    local output_name="colormc-macos-$version-$2"
    
    echo "build DMG: $output_name"
    
    local args=(
        "--volname"
        "${app_name} Installer"
        "--icon-size"
        "100"
        "--icon"
        "${app_name}.app"
        "180"
        "170"
        "--window-pos"
        "200"
        "200"
        "--window-size"
        "600"
        "400"
        "--app-drop-link"
        "430"
        "170"
    )
    
    if [ -n "$background" ] && [ -f "$background" ]; then
        args+=("--background \"$background\"")
    fi

    "$CREATE_DMG_SCRIPT" "${args[@]}" "./build_out/${output_name}.dmg" "$app_path"
}

build_min() {
    local app_name="ColorMC"
    local app_path="./src/build_out/$1-min/ColorMC.app"
    local background=${2:-""}  # 可选背景图
    local output_name="colormc-macos-$version-min-$2"
    
    echo "build DMG: $output_name"
    
    local args=(
        "--volname"
        "${app_name} Installer"
        "--icon-size"
        "100"
        "--icon"
        "${app_name}.app"
        "180"
        "170"
        "--window-pos"
        "200"
        "200"
        "--window-size"
        "600"
        "400"
        "--app-drop-link"
        "430"
        "170"
    )
    
    if [ -n "$background" ] && [ -f "$background" ]; then
        args+=("--background \"$background\"")
    fi

    "$CREATE_DMG_SCRIPT" "${args[@]}" "./build_out/${output_name}.dmg" "$app_path"
}

# 下载 create-dmg（只下载一次）
download_create_dmg
if [ $? -ne 0 ]; then
    echo "download create-dmg fail"
    exit 1
fi

build osx-arm64 aarch64
build osx-x64 x86_64
build_min osx-arm64 aarch64
build_min osx-x64 x86_64
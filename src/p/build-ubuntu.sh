dpkg -b linux-amd64 colormc-linux-amd64.deb
dpkg -b linux-arm64 colormc-linux-arm64.deb
./deb2appimage-0.0.5-x86_64.AppImage -j appimg.json -o ./
sudo dpkg -b linux-amd64 colormc-A19-linux-amd64.deb
sudo dpkg -b linux-arm64 colormc-A19-linux-arm64.deb
sudo ./deb2appimage-0.0.5-x86_64.AppImage -j appimg-amd64.json -o ./
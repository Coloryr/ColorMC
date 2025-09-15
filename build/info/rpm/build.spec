name:       colormc
Version:    %version%
Release:    1
Summary:    A Minecraft Launcher
License:    Apache-2.0

%description
A Minecraft Launcher

%pre
rm -f /usr/local/bin/ColorMC.Launcher

%post
chmod a+x /usr/share/colormc/ColorMC.Launcher
chmod a+x /usr/share/applications/ColorMC.desktop
ln -s /usr/share/colormc/ColorMC.Launcher /usr/local/bin
update-mime-database /usr/share/mime
xdg-settings set default-url-scheme-handler colormc ColorMC.desktop

%postup
update-desktop-database /usr/share/applications

%files
/usr/share/applications/ColorMC.desktop
/usr/share/colormc/
/usr/share/icons/colormc.png
/usr/share/mime/packages/colormc.xml

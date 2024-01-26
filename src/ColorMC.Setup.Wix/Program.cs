using System;
using System.Windows.Forms;
using WixSharp;
using WixToolset.Dtf.WindowsInstaller;

namespace ColorMC.Setup.Wix
{
    internal class Program
    {
        static void Main()
        {
            var project = new ManagedProject("ColorMC",
                        new Dir(@"%ProgramFiles%\ColorMC",
                            new File(@"..\build_out\win-x64-dotnet\ColorMC.Launcher.exe", 
                                new FileShortcut("ColorMC") { WorkingDirectory = "[INSTALLDIR]" }),
                            new File(@"..\build_out\win-x64-dotnet\ColorMC.Core.pdb"),
                            new File(@"..\build_out\win-x64-dotnet\ColorMC.Gui.pdb"),
                            new File(@"..\build_out\win-x64-dotnet\Live2DCSharpSDK.App.pdb"),
                            new File(@"..\build_out\win-x64-dotnet\Live2DCSharpSDK.Framework.pdb"),
                            new File(@"..\build_out\win-x64-dotnet\ColorMC.Launcher.pdb"),
                            new ExeFileShortcut("ColorMC Setting", "[System64Folder]msiexec.exe", "/i [ProductCode]")),
                        new Dir(@"%ProgramMenu%\ColorMC",
                            new ExeFileShortcut("ColorMC Setting", "[System64Folder]msiexec.exe", "/i [ProductCode]"),
                            new ExeFileShortcut("ColorMC", "[INSTALLDIR]ColorMC.Launcher.exe", arguments: "")),
                        new Dir(@"%Desktop%",
                            new ExeFileShortcut("ColorMC", "[INSTALLDIR]ColorMC.Launcher.exe", arguments: "")
                            {
                                Condition = new Condition("INSTALLDESKTOPSHORTCUT=\"yes\"") //property based condition
                            }),
                        //setting property to be used in install condition
                        new Property("INSTALLDESKTOPSHORTCUT", "yes"),
                        new Property("REMOVE_ALL_FILE", "false"))
            {
                GUID = new Guid("BA2749D2-BBA4-4ACE-8E06-C4E100343C1A"),
                Platform = Platform.x64,
                BannerImage = "game.png",
                BackgroundImage = "game.png",
                Version = new Version(1, 24),
                Description = "A Minecraft Launcher"
            };

            project.Scope = InstallScope.perMachine;

            project.ControlPanelInfo.Comments = "ColorMC";
            project.ControlPanelInfo.HelpLink = "https://github.com/Coloryr/ColorMC";
            project.ControlPanelInfo.UrlInfoAbout = "https://github.com/Coloryr/ColorMC";
            project.ControlPanelInfo.ProductIcon = "icon.ico";
            project.ControlPanelInfo.Manufacturer = "Coloryr";
            project.ControlPanelInfo.InstallLocation = "[INSTALLDIR]";

            project.ManagedUI = new ManagedUI();

            project.ManagedUI.InstallDialogs.Add<WelcomeDialog>()
                                            .Add<ProgressDialog>()
                                            .Add<ExitDialog>();

            project.ManagedUI.ModifyDialogs.Add<MaintenanceTypeDialog>()
                                           .Add<ProgressDialog>()
                                           .Add<ExitDialog>();
            project.OutFileName = "ColorMC";
            project.BuildMsi();

            project = new ManagedProject("ColorMC",
                        new Dir(@"%ProgramFiles%\ColorMC",
                            new File(@"..\build_out\win-x64-aot\ColorMC.Launcher.exe", 
                                new FileShortcut("ColorMC") { WorkingDirectory = "[INSTALLDIR]" }),
                            new File(@"..\build_out\win-x64-aot\av_libglesv2.dll"),
                            new File(@"..\build_out\win-x64-aot\libHarfBuzzSharp.dll"),
                            new File(@"..\build_out\win-x64-aot\libSkiaSharp.dll"),
                            new File(@"..\build_out\win-x64-aot\SDL2.dll"),
                            new ExeFileShortcut("ColorMC Setting", "[System64Folder]msiexec.exe", "/i [ProductCode]")),
                        new Dir(@"%ProgramMenu%\ColorMC",
                            new ExeFileShortcut("ColorMC Setting", "[System64Folder]msiexec.exe", "/i [ProductCode]"),
                            new ExeFileShortcut("ColorMC", "[INSTALLDIR]ColorMC.Launcher.exe", arguments: "")),
                        new Dir(@"%Desktop%",
                            new ExeFileShortcut("ColorMC", "[INSTALLDIR]ColorMC.Launcher.exe", arguments: "")
                            {
                                Condition = new Condition("INSTALLDESKTOPSHORTCUT=\"yes\"") //property based condition
                            }),
                        //setting property to be used in install condition
                        new Property("INSTALLDESKTOPSHORTCUT", "yes"),
                        new Property("REMOVE_ALL_FILE", "false"))
            {
                GUID = new Guid("BA2749D2-BBA4-4ACE-8E06-C4E100343C1A"),
                Platform = Platform.x64,
                BannerImage = "game.png",
                BackgroundImage = "game.png",
                Version = new Version(1, 24),
                Description = "A Minecraft Launcher"
            };

            project.Scope = InstallScope.perMachine;

            project.ControlPanelInfo.Comments = "ColorMC";
            project.ControlPanelInfo.HelpLink = "https://github.com/Coloryr/ColorMC";
            project.ControlPanelInfo.UrlInfoAbout = "https://github.com/Coloryr/ColorMC";
            project.ControlPanelInfo.ProductIcon = "icon.ico";
            project.ControlPanelInfo.Manufacturer = "Coloryr";
            project.ControlPanelInfo.InstallLocation = "[INSTALLDIR]";

            project.ManagedUI = new ManagedUI();

            project.ManagedUI.InstallDialogs.Add<WelcomeDialog>()
                                            .Add<ProgressDialog>()
                                            .Add<ExitDialog>();

            project.ManagedUI.ModifyDialogs.Add<MaintenanceTypeDialog>()
                                           .Add<ProgressDialog>()
                                           .Add<ExitDialog>();

            project.OutFileName = "ColorMC-aot";
            project.BuildMsi();
        }
    }
}
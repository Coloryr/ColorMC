using System;
using WixSharp;

namespace ColorMC.Setup.Wix
{
    internal class Program
    {
        private static Dir BuildX64()
        {
            return new Dir(@"%ProgramFiles%\ColorMC",
                            new File(@"..\build_out\win-x64-dotnet\ColorMC.Launcher.exe",
                                new FileShortcut("ColorMC") { WorkingDirectory = "[INSTALLDIR]" }),
                            new File(@"..\build_out\win-x64-dotnet\ColorMC.Core.pdb"),
                            new File(@"..\build_out\win-x64-dotnet\ColorMC.Gui.pdb"),
                            new File(@"..\build_out\win-x64-dotnet\Live2DCSharpSDK.App.pdb"),
                            new File(@"..\build_out\win-x64-dotnet\Live2DCSharpSDK.Framework.pdb"),
                            new File(@"..\build_out\win-x64-dotnet\ColorMC.Launcher.pdb"),
                            new File(@"..\build_out\win-x64-dotnet\av_libglesv2.dll"),
                            new File(@"..\build_out\win-x64-dotnet\libHarfBuzzSharp.dll"),
                            new File(@"..\build_out\win-x64-dotnet\libSkiaSharp.dll"),
                            new File(@"..\build_out\win-x64-dotnet\SDL2.dll"),
                            new File(@"..\build_out\win-x64-dotnet\X11.pdb"),
                            new ExeFileShortcut("ColorMC Setting", "[System64Folder]msiexec.exe", "/i [ProductCode]"));
        }

        private static Dir BuildX64Min()
        {
            return new Dir(@"%ProgramFiles%\ColorMC",
                            new File(@"..\build_out\win-x64-min\ColorMC.Launcher.exe",
                                new FileShortcut("ColorMC") { WorkingDirectory = "[INSTALLDIR]" }),
                            new File(@"..\build_out\win-x64-min\ColorMC.Core.pdb"),
                            new File(@"..\build_out\win-x64-min\ColorMC.Gui.pdb"),
                            new File(@"..\build_out\win-x64-min\Live2DCSharpSDK.App.pdb"),
                            new File(@"..\build_out\win-x64-min\Live2DCSharpSDK.Framework.pdb"),
                            new File(@"..\build_out\win-x64-min\ColorMC.Launcher.pdb"),
                            new File(@"..\build_out\win-x64-min\av_libglesv2.dll"),
                            new File(@"..\build_out\win-x64-min\libHarfBuzzSharp.dll"),
                            new File(@"..\build_out\win-x64-min\libSkiaSharp.dll"),
                            new File(@"..\build_out\win-x64-min\SDL2.dll"),
                            new File(@"..\build_out\win-x64-min\X11.pdb"),
                            new ExeFileShortcut("ColorMC Setting", "[System64Folder]msiexec.exe", "/i [ProductCode]"));
        }

        private static Dir BuildX64AOT()
        {
            return new Dir(@"%ProgramFiles%\ColorMC",
                            new File(@"..\build_out\win-x64-aot\ColorMC.Launcher.exe",
                                new FileShortcut("ColorMC") { WorkingDirectory = "[INSTALLDIR]" }),
                            new File(@"..\build_out\win-x64-aot\av_libglesv2.dll"),
                            new File(@"..\build_out\win-x64-aot\libHarfBuzzSharp.dll"),
                            new File(@"..\build_out\win-x64-aot\libSkiaSharp.dll"),
                            new File(@"..\build_out\win-x64-aot\SDL2.dll"),
                            new ExeFileShortcut("ColorMC Setting", "[System64Folder]msiexec.exe", "/i [ProductCode]"));
        }

        //private static Dir BuildArm64()
        //{
        //    return new Dir(@"%ProgramFiles%\ColorMC",
        //                    new File(@"..\build_out\win-arm64-dotnet\ColorMC.Launcher.exe",
        //                        new FileShortcut("ColorMC") { WorkingDirectory = "[INSTALLDIR]" }),
        //                    new File(@"..\build_out\win-arm64-dotnet\ColorMC.Core.pdb"),
        //                    new File(@"..\build_out\win-arm64-dotnet\ColorMC.Gui.pdb"),
        //                    new File(@"..\build_out\win-arm64-dotnet\Live2DCSharpSDK.App.pdb"),
        //                    new File(@"..\build_out\win-arm64-dotnet\Live2DCSharpSDK.Framework.pdb"),
        //                    new File(@"..\build_out\win-arm64-dotnet\ColorMC.Launcher.pdb"),
        //                    new ExeFileShortcut("ColorMC Setting", "[System64Folder]msiexec.exe", "/i [ProductCode]"));
        //}

        //private static Dir BuildArm64AOT()
        //{
        //    return new Dir(@"%ProgramFiles%\ColorMC",
        //                    new File(@"..\build_out\win-arm64-aot\ColorMC.Launcher.exe",
        //                        new FileShortcut("ColorMC") { WorkingDirectory = "[INSTALLDIR]" }),
        //                    new File(@"..\build_out\win-arm64-aot\av_libglesv2.dll"),
        //                    new File(@"..\build_out\win-arm64-aot\libHarfBuzzSharp.dll"),
        //                    new File(@"..\build_out\win-arm64-aot\libSkiaSharp.dll"),
        //                    new File(@"..\build_out\win-arm64-aot\SDL2.dll"),
        //                    new ExeFileShortcut("ColorMC Setting", "[System64Folder]msiexec.exe", "/i [ProductCode]"));
        //}

        private static void Build(Dir dir, string file, Platform platform)
        {
            WixExtension.UI.PreferredVersion = "4.0.4"; // or any other working version
            var project = new ManagedProject("ColorMC",
                        dir,
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
                Platform = platform,
                BannerImage = "game.png",
                BackgroundImage = "game.png",
                Version = new Version(1, 28),
                Description = "A Minecraft Launcher"
            };

            project.Scope = InstallScope.perMachine;

            if (platform == Platform.arm64)
            {
                project.InstallerVersion = 500;
            }
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
            project.OutFileName = file;
            project.BuildMsi();
        }

        static void Main()
        {
            Build(BuildX64(), "colormc-x64", Platform.x64);
            Build(BuildX64AOT(), "colormc-x64-aot", Platform.x64);
            Build(BuildX64Min(), "colormc-x64-min", Platform.x64);
            //Build(BuildArm64(), "colormc-arm64", Platform.arm64);
            //Build(BuildArm64AOT(), "colormc-arm64-aot", Platform.arm64);
            //Build(BuildX64Min(), "colormc-x64-min", Platform.arm64);
        }
    }
}
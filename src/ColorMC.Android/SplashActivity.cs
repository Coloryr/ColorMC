using Android.App;
using Android.Content;
using Android.OS;
using Application = Android.App.Application;
using Avalonia;
using Avalonia.Android;
using ColorMC.Gui;
using System;
using ColorMC.Core.Utils;
using ColorMC.Core;

namespace ColorMC.Android;

[Activity(Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true)]
public class SplashActivity : AvaloniaSplashActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        try
        {
            SystemInfo.Init();

            var data = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "/ColorMC/";

            ColorMCCore.Init(data);
            GuiConfigUtils.Init(data);
            ImageTemp.Init(data);
        }
        catch (Exception e)
        { 
            
        }

        return base.CustomizeAppBuilder(builder);
    }

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
    }

    protected override void OnResume()
    {
        base.OnResume();

        StartActivity(new Intent(Application.Context, typeof(MainActivity)));
    }
}

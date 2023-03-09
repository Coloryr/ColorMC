using Android.App;
using Android.Content;
using Android.OS;
using Application = Android.App.Application;
using Avalonia;
using Avalonia.Android;
using ColorMC.Gui;
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

            var temp = GetExternalFilesDir(null) + "/";

            try
            {
                System.Diagnostics.Process p = new();
                p.StartInfo.FileName = "sh";
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.Start();

                p.StandardInput.WriteLine("chmod -R 777 " + temp);
                p.StandardInput.WriteLine("exit");
                p.WaitForExit();

                string temp1 = p.StandardOutput.ReadToEnd();

                p.Dispose();
            }
            catch (System.Exception e)
            {
                Logs.Error("java chmod fail", e);
            }

            ColorMCCore.Init(temp);
            GuiConfigUtils.Init(temp);
            ImageTemp.Init(temp);
        }
        catch (System.Exception e)
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

using Android.App;
using Android.Content.PM;
using Android.OS;
using Avalonia.Android;

namespace ColorMC.Android;

[Activity(Label = "temp.Android", 
    Theme = "@style/MyTheme.NoActionBar", 
    Icon = "@drawable/icon", 
    LaunchMode = LaunchMode.SingleTop, 
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize, 
    ScreenOrientation = ScreenOrientation.SensorLandscape)]
public class MainActivity : AvaloniaMainActivity
{
    protected void OnCreate(Bundle savedInstanceState)
    {

        base.OnCreate(savedInstanceState);
    }
}

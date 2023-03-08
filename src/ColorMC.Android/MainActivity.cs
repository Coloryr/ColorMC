using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace ColorMC.Android;

[Activity(Label = "temp.Android", 
    Theme = "@style/MyTheme.NoActionBar", 
    Icon = "@drawable/icon", 
    LaunchMode = LaunchMode.SingleTop, 
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize, 
    ScreenOrientation = ScreenOrientation.Landscape)]
public class MainActivity : AvaloniaMainActivity
{
}

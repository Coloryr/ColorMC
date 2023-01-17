using ColorMC.Cmd.Menus;
using ColorMC.Core;

namespace ColorMC.Cmd;

internal class Program
{
    static void Main(string[] args)
    {
        ConsoleUtils.Init();
        CoreMain.Init(AppContext.BaseDirectory);
        Thread.Sleep(1000);

        DownloadBar.Init();

        MainMenu.Show();
    }
}
using ColorMC.Cmd.Menus;
using ColorMC.Core;

namespace ColorMC.Cmd;

internal class Program
{
    static void Main(string[] args)
    {
        ConsoleUtils.Init();
        CoreMain.Init("E:\\code\\ColorMC\\buildout\\");
        Thread.Sleep(1000);
        MainMenu.Show();
    }
}
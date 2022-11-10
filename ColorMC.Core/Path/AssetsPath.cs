namespace ColorMC.Core.Path;

public static class AssetsPath
{
    private const string Name = "assets";

    private const string Name1 = "indexes";
    private const string Name2 = "objects";
    private const string Name3 = "skins";

    public static string BaseDir { get; set; }

    public static void InitPath(string dir) 
    {
        BaseDir = dir + "/" + Name;

        Directory.CreateDirectory(BaseDir);

        Directory.CreateDirectory($"{BaseDir}/{Name1}");
        Directory.CreateDirectory($"{BaseDir}/{Name2}");
        Directory.CreateDirectory($"{BaseDir}/{Name3}");
    }
}
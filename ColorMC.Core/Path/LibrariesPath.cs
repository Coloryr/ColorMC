namespace ColorMC.Core.Path;

public static class LibrariesPath
{
    private const string Name = "libraries";

    public static string BaseDir { get; private set; }

    public static void Init(string dir)
    {
        BaseDir = dir + "/" + Name;

        Directory.CreateDirectory(BaseDir);
    }
}

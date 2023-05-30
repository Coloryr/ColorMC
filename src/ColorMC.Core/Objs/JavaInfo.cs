using ColorMC.Core.LaunchPath;

namespace ColorMC.Core.Objs;

public record JavaInfo
{
    public string Path { get; set; }
    public string Version { get; set; }
    public int MajorVersion
    {
        get
        {
            if (Version == JvmPath.Unknow)
            {
                return -1;
            }
            string[] vers = Version.Trim().Split('.', '_', '-', '+', 'u', 'U');
            if (vers[0] == "1")
            {
                if (int.TryParse(vers[1], out var data))
                {
                    return data;
                }

                return 0;
            }
            else
            {
                if(int.TryParse(vers[0], out var data))
                {
                    return data;
                }

                return 0;
            }
        }
    }
    public string Type { get; set; }
    public ArchEnum Arch { get; set; }
}

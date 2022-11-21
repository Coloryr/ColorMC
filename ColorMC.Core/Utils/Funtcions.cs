using System.Security.Cryptography;
using System.Text;

namespace ColorMC.Core.Utils;

public static class Funtcions
{
    public static string GenSha1(Stream stream)
    {
        SHA1 sha1 = SHA1.Create();
        StringBuilder EnText = new();
        foreach (byte iByte in sha1.ComputeHash(stream))
        {
            EnText.AppendFormat("{0:x2}", iByte);
        }
        return EnText.ToString().ToLower();
    }

    public static string NewUUID()
    {
        return Guid.NewGuid().ToString().ToLower();
    }
}

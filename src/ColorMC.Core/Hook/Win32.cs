using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Hook;

public static class Win32
{
    // SHFILEOPSTRUCT结构体用于定义文件操作的参数
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct SHFILEOPSTRUCT
    {
        public IntPtr hwnd;
        [MarshalAs(UnmanagedType.U4)]
        public int wFunc;
        public string pFrom;
        public string pTo;
        public short fFlags;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fAnyOperationsAborted;
        public IntPtr hNameMappings;
        public string lpszProgressTitle;
    }

    // 定义文件操作的常量
    private const int FO_DELETE = 3;
    private const int FOF_ALLOWUNDO = 0x40; // 允许撤销，即移动到回收站

    // 调用SHFileOperation函数
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

    public static bool MoveToTrash(string dir)
    {
        // 设置SHFILEOPSTRUCT结构体
        SHFILEOPSTRUCT fileOp = new SHFILEOPSTRUCT
        {
            wFunc = FO_DELETE,
            pFrom = dir + '\0' + '\0', // 双重终止符
            fFlags = FOF_ALLOWUNDO
        };

        // 调用SHFileOperation函数
        int result = SHFileOperation(ref fileOp);

        return result == 0;
    }
}

using System.Runtime.InteropServices;

namespace ColorMC.Core.Hook;

public static partial class Win32
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
    private const ushort FOF_ALLOWUNDO = 0x40; // 允许撤销，即移动到回收站
    private const ushort FOF_NOCONFIRMATION = 0x10; // 不显示确认对话框
    private const ushort FOF_SILENT = 0x4; // 不显示进度

    // 调用SHFileOperation函数
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

    public static bool MoveToTrash(string path)
    {
        // 设置SHFILEOPSTRUCT结构体
        SHFILEOPSTRUCT fs = new SHFILEOPSTRUCT
        {
            wFunc = FO_DELETE,
            // 确保字符串以双空字符结尾
            pFrom = path + '\0' + '\0',
            fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION | FOF_SILENT,
            fAnyOperationsAborted = false,
            hNameMappings = IntPtr.Zero,
            lpszProgressTitle = string.Empty
        };

        // 调用SHFileOperation函数
        int result = SHFileOperation(ref fs);

        return result == 0;
    }
}

//using System.ComponentModel;
//using System.Diagnostics;
//using System.Text;
//using Microsoft.Win32.SafeHandles;
//public static partial class Win32
//{
//    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
//    private static extern bool CreateProcess(
//            string lpApplicationName,
//            string lpCommandLine,
//            ref SECURITY_ATTRIBUTES lpProcessAttributes,
//            ref SECURITY_ATTRIBUTES lpThreadAttributes,
//            bool bInheritHandles,
//            uint dwCreationFlags,
//            IntPtr lpEnvironment,
//            string lpCurrentDirectory,
//            ref STARTUPINFOEX lpStartupInfoEx,
//            out PROCESS_INFORMATION lpProcessInformation);

//    [DllImport("kernel32.dll", SetLastError = true)]
//    private static extern bool InitializeProcThreadAttributeList(
//       IntPtr lpAttributeList,
//       int dwAttributeCount,
//       int dwFlags,
//       ref IntPtr lpSize);

//    [DllImport("kernel32.dll", SetLastError = true)]
//    private static extern bool UpdateProcThreadAttribute(
//       IntPtr lpAttributeList,
//       uint dwFlags,
//       IntPtr Attribute,
//       IntPtr lpValue,
//       IntPtr cbSize,
//       IntPtr lpPreviousValue,
//       IntPtr lpReturnSize);

//    [DllImport("kernel32.dll", SetLastError = true)]
//    private static extern bool CloseHandle(IntPtr hObject);

//    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
//    private static extern bool CreateAppContainerProfile(
//        string pszAppContainerName,
//        string pszDisplayName,
//        string pszDescription,
//        IntPtr pSidCapabilities,
//        uint dwCapabilityCount,
//        out IntPtr pSidAppContainerSid);

//    [DllImport("advapi32.dll", SetLastError = true)]
//    private static extern bool DeleteAppContainerProfile(string pszAppContainerName);

//    [DllImport("advapi32.dll", SetLastError = true)]
//    private static extern bool DeriveAppContainerSidFromAppContainerName(
//        string pszAppContainerName,
//        out IntPtr pSidAppContainerSid);

//    [DllImport("kernel32.dll", SetLastError = true)]
//    private static extern IntPtr LocalFree(IntPtr hMem);

//    [DllImport("kernel32.dll", SetLastError = true)]
//    private static extern bool CreatePipe(
//        out IntPtr hReadPipe,
//        out IntPtr hWritePipe,
//        ref SECURITY_ATTRIBUTES lpPipeAttributes,
//        int nSize);

//    [DllImport("kernel32.dll", SetLastError = true)]
//    private static extern bool SetHandleInformation(IntPtr hObject, uint dwMask, uint dwFlags);

//    [StructLayout(LayoutKind.Sequential)]
//    private struct SECURITY_ATTRIBUTES
//    {
//        public int nLength;
//        public IntPtr lpSecurityDescriptor;
//        public bool bInheritHandle;
//    }

//    [StructLayout(LayoutKind.Sequential)]
//    private struct STARTUPINFO
//    {
//        public int cb;
//        public string lpReserved;
//        public string lpDesktop;
//        public string lpTitle;
//        public int dwX;
//        public int dwY;
//        public int dwXSize;
//        public int dwYSize;
//        public int dwXCountChars;
//        public int dwYCountChars;
//        public int dwFillAttribute;
//        public int dwFlags;
//        public short wShowWindow;
//        public short cbReserved2;
//        public IntPtr lpReserved2;
//        public IntPtr hStdInput;
//        public IntPtr hStdOutput;
//        public IntPtr hStdError;
//    }

//    [StructLayout(LayoutKind.Sequential)]
//    private struct PROCESS_INFORMATION
//    {
//        public IntPtr hProcess;
//        public IntPtr hThread;
//        public int dwProcessId;
//        public int dwThreadId;
//    }

//    [StructLayout(LayoutKind.Sequential)]
//    private struct SECURITY_CAPABILITIES
//    {
//        public IntPtr AppContainerSid;
//        public IntPtr Capabilities;
//        public uint CapabilityCount;
//        public uint Reserved;
//    }

//    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
//    private struct STARTUPINFOEX
//    {
//        public STARTUPINFO StartupInfo;
//        public IntPtr lpAttributeList;
//    }

//    private const uint HANDLE_FLAG_INHERIT = 0x00000001;
//    private const uint CREATE_NO_WINDOW = 0x08000000;

//    public static void Start()
//    {
//        string appContainerName = "MyAppContainer";
//        string applicationPath = @"C:\Windows\System32\notepad.exe";

//        IntPtr appContainerSid = IntPtr.Zero;
//        IntPtr attributeList = IntPtr.Zero;

//        // Create pipes for standard input, output, and error
//        IntPtr hStdInputRead, hStdInputWrite;
//        IntPtr hStdOutputRead, hStdOutputWrite;
//        IntPtr hStdErrorRead, hStdErrorWrite;

//        // Create AppContainer profile
//        bool created = CreateAppContainerProfile(
//            appContainerName,
//            "My AppContainer",
//            "This is a sample AppContainer profile",
//            IntPtr.Zero,
//            0,
//            out appContainerSid);

//        if (!created)
//        {
//            int error = Marshal.GetLastWin32Error();
//            if (error == 0x522) // ERROR_ALREADY_EXISTS
//            {
//                Console.WriteLine("AppContainer already exists. Reusing existing AppContainer.");
//                DeriveAppContainerSidFromAppContainerName(appContainerName, out appContainerSid);
//            }
//            else
//            {
//                throw new Win32Exception(error, "Failed to create AppContainer profile.");
//            }
//        }

//        // Define SECURITY_CAPABILITIES
//        SECURITY_CAPABILITIES securityCapabilities = new SECURITY_CAPABILITIES
//        {
//            AppContainerSid = appContainerSid,
//            Capabilities = IntPtr.Zero,
//            CapabilityCount = 0,
//            Reserved = 0
//        };

//        SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
//        sa.nLength = Marshal.SizeOf(sa);
//        sa.bInheritHandle = true;

//        // Create pipes for stdin
//        if (!CreatePipe(out hStdInputRead, out hStdInputWrite, ref sa, 0))
//            throw new Win32Exception(Marshal.GetLastWin32Error());

//        SetHandleInformation(hStdInputWrite, HANDLE_FLAG_INHERIT, 0);

//        // Create pipes for stdout
//        if (!CreatePipe(out hStdOutputRead, out hStdOutputWrite, ref sa, 0))
//            throw new Win32Exception(Marshal.GetLastWin32Error());
//        SetHandleInformation(hStdOutputRead, HANDLE_FLAG_INHERIT, 0);

//        // Create pipes for stderr
//        if (!CreatePipe(out hStdErrorRead, out hStdErrorWrite, ref sa, 0))
//            throw new Win32Exception(Marshal.GetLastWin32Error());
//        SetHandleInformation(hStdErrorRead, HANDLE_FLAG_INHERIT, 0);

//        // Set up STARTUPINFOEX
//        STARTUPINFOEX startupInfoEx = new STARTUPINFOEX();
//        startupInfoEx.StartupInfo.cb = Marshal.SizeOf(startupInfoEx);
//        startupInfoEx.StartupInfo.dwFlags = 0x00000100; // STARTF_USESTDHANDLES
//        startupInfoEx.StartupInfo.hStdInput = hStdInputRead;
//        startupInfoEx.StartupInfo.hStdOutput = hStdOutputWrite;
//        startupInfoEx.StartupInfo.hStdError = hStdErrorWrite;

//        // Calculate size of attribute list
//        IntPtr size = IntPtr.Zero;
//        InitializeProcThreadAttributeList(IntPtr.Zero, 3, 0, ref size);
//        attributeList = Marshal.AllocHGlobal(size);

//        // Initialize attribute list
//        if (!InitializeProcThreadAttributeList(attributeList, 3, 0, ref size))
//        {
//            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to initialize attribute list.");
//        }

//        // Update attribute list with SECURITY_CAPABILITIES
//        IntPtr securityCapabilitiesPtr = Marshal.AllocHGlobal(Marshal.SizeOf(securityCapabilities));
//        Marshal.StructureToPtr(securityCapabilities, securityCapabilitiesPtr, false);

//        if (!UpdateProcThreadAttribute(attributeList, 0, 0x20007, securityCapabilitiesPtr, Marshal.SizeOf(securityCapabilities), IntPtr.Zero, IntPtr.Zero))
//        {
//            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to update attribute list with SECURITY_CAPABILITIES.");
//        }

//        // Set up STARTUPINFOEX
//        startupInfoEx.lpAttributeList = attributeList;

//        // Set up PROCESS_INFORMATION structure
//        PROCESS_INFORMATION pi = new PROCESS_INFORMATION();
//        try
//        {
//            // Start the process
//            bool success = CreateProcess(
//                null, // Application name
//                applicationPath, // Command line
//                ref sa, // Process security attributes
//                ref sa, // Thread security attributes
//                true, // Inherit handles
//                0x00080000, // EXTENDED_STARTUPINFO_PRESENT
//                IntPtr.Zero, // Environment
//                null, // Current directory
//                ref startupInfoEx, // Startup info
//                out pi // Process info
//            );

//            if (!success)
//            {
//                throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to create process.");
//            }

//            // Example: Write to stdin
//            using var stream = new FileStream(new SafeFileHandle(hStdInputWrite, true), FileAccess.Write);
//            byte[] input = Encoding.UTF8.GetBytes("Hello from parent process!\n");
//            stream.Write(input, 0, input.Length);

//            // Example: Read from stdout
//            using var stream1 = new FileStream(new SafeFileHandle(hStdOutputRead, true), FileAccess.Read);
//            byte[] buffer = new byte[1024];
//            int bytesRead = stream1.Read(buffer, 0, buffer.Length);
//            Console.WriteLine("Output from child process:");
//            Console.WriteLine(Encoding.UTF8.GetString(buffer, 0, bytesRead));

//            // Wait for process to exit
//            Process process = Process.GetProcessById(pi.dwProcessId);
//            process.WaitForExit();
//        }
//        finally
//        {
//            // Clean up handles
//            CloseHandle(hStdInputRead);
//            CloseHandle(hStdInputWrite);
//            CloseHandle(hStdOutputRead);
//            CloseHandle(hStdOutputWrite);
//            CloseHandle(hStdErrorRead);
//            CloseHandle(hStdErrorWrite);
//            CloseHandle(pi.hProcess);
//            CloseHandle(pi.hThread);

//            // Free SID
//            if (appContainerSid != IntPtr.Zero)
//            {
//                LocalFree(appContainerSid);
//            }

//            // Delete AppContainer profile
//            DeleteAppContainerProfile(appContainerName);
//        }
//    }
//}
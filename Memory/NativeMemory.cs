using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace obisoft.net.memory
{
    public class NativeMemory
    {

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(ProcessOperation dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess,
          int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(int hProcess, IntPtr lpBaseAddress,
            byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetDllDirectory(string lpPathName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr MemSet(IntPtr dest, int c, int byteCount);
    }
}

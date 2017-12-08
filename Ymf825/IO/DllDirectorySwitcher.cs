using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Ymf825.IO
{
    internal static class DllDirectorySwitcher
    {
        public static void Apply()
        {
            var executionDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? ".";
            SetDllDirectory(null);

            SetDllDirectory(Environment.Is64BitProcess
                ? Path.Combine(executionDirectory, "lib", "x64")
                : Path.Combine(executionDirectory, "lib", "x86"));
        }

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);
    }
}

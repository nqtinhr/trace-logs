using System.Runtime.InteropServices;

namespace DemoApp
{
    internal static class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [STAThread]
        static void Main()
        {
            //AllocConsole(); // Bật console
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}
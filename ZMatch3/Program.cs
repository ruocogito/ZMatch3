using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using System.Runtime.InteropServices;
using System;

namespace ZMatch3
{
    class Program
    {
        
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;


        static void Main(string[] args)
        {
            var handle = GetConsoleWindow();

            // Hide console window
            ShowWindow(handle, SW_HIDE);

            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(800, 600),
                Title = "ZMatch3",
            };
            

            // To create a new window, create a class that extends GameWindow, then call Run() on it.
            using (var window  = new zGame(GameWindowSettings.Default, nativeWindowSettings))
            {
                window.UpdateFrequency = 60;
                window.RenderFrequency = 60;
                window.WindowBorder = OpenTK.Windowing.Common.WindowBorder.Hidden;
                window.Run();
            }

        }
    }
}

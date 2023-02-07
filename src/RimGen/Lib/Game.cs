using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RimGen.Lib
{
    public class Game
    {
        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);

        private enum ShowWindowEnum
        {
            Hide = 0,
            ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
            Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
            Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
            Restore = 9, ShowDefault = 10, ForceMinimized = 11
        };


        [DllImport("User32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint nFlags);

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr handle, ref Rectangle rect);

        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
        static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);

        private static IntPtr gameWindowHandle;

        public static void Init()
        {
            GetWindowByName("RimWorld by Ludeon Studios");
            ShowWindow(gameWindowHandle, ShowWindowEnum.Restore);
        }

        public static void SendGenerateNew(Point genBtnCenterPos)
        {
            //ShowWindow(gameWindowHandle, ShowWindowEnum.Restore);
            Clicker.ClickOnPoint(gameWindowHandle, genBtnCenterPos);
        }

        public static void SaveScreenshot()
        {
            var bmp = GetScreenshot();

            string screenshotPath = ("rimworld.bmp");
            bmp.Save(screenshotPath, ImageFormat.Bmp);
            bmp.Dispose();
        }

        public static Bitmap GetScreenshot()
        {
            var bounds = new Rectangle();
            GetWindowRect(gameWindowHandle, ref bounds);

            var bitmap = new Bitmap(bounds.Width, bounds.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                //IntPtr hdc = g.GetHdc();
                //PrintWindow(gameWindowHandle, hdc, 0);
                //g.ReleaseHdc(hdc);
                g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
            }

            return bitmap;
        }

        private static IntPtr GetWindowByName(string wName)
        {
            IntPtr hWnd = IntPtr.Zero;
            foreach (Process pList in Process.GetProcesses())
            {
                if (pList.MainWindowTitle.Contains(wName))
                {
                    hWnd = pList.MainWindowHandle;
                }
            }
            gameWindowHandle = hWnd;

            if (hWnd == IntPtr.Zero) {
                throw new Exception(Form1.Lang == "ru" ? "Ошибка! Окно с игрой не найдено" : "Error! Game window not found");
            }

            return hWnd;
        }
    }
}

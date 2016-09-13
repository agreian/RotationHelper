using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Windows;

namespace RotationHelper
{
    internal class ScreenshotHelper
    {
        #region Fields

        private static readonly List<Color> Colors = new List<Color>();

        #endregion

        #region Methods

        public static Color[] GetColorAt(params System.Drawing.Point[] points)
        {
            Colors.Clear();

            var desk = GetDesktopWindow();
            var dc = GetWindowDC(desk);

            foreach (var point in points)
            {
                var pixel = (int)GetPixel(dc, point.X, point.Y);
                Colors.Add(Color.FromArgb(255, (byte)((pixel >> 0) & 0xFF), (byte)((pixel >> 8) & 0xFF), (byte)((pixel >> 16) & 0xFF)));
            }

            ReleaseDC(desk, dc);

            return Colors.ToArray();
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern uint GetPixel(IntPtr dc, int x, int y);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindowDC(IntPtr window);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int ReleaseDC(IntPtr window, IntPtr dc);

        #endregion
    }
}
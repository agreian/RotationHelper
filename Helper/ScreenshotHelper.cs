using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using Color = System.Windows.Media.Color;

namespace RotationHelper.Helper
{
    internal class ScreenshotHelper
    {
        #region Fields

        private static readonly List<Color> _colors = new List<Color>();

        #endregion

        #region Methods

        public static Color[] GetColorAt(params Point[] points)
        {
            _colors.Clear();

            var desktopWindow = GetDesktopWindow();
            var windowDc = GetWindowDC(desktopWindow);

            foreach (var point in points)
            {
                var pixel = (int)GetPixel(windowDc, point.X, point.Y);
                _colors.Add(Color.FromArgb(255, (byte)((pixel >> 0) & 0xFF), (byte)((pixel >> 8) & 0xFF), (byte)((pixel >> 16) & 0xFF)));
            }

            ReleaseDC(desktopWindow, windowDc);

            return _colors.ToArray();
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
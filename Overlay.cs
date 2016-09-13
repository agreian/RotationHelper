using System;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows.Forms;
using RotationHelper.Model;
using RotationHelper.ViewModel;
using Timer = System.Timers.Timer;

namespace RotationHelper
{
    public partial class Overlay
    {
        #region Enums

        public enum GetWindowLongConst
        {
            GwlWndproc = -4,
            GwlHinstance = -6,
            GwlHwndparent = -8,
            GwlStyle = -16,
            GwlExstyle = -20,
            GwlUserdata = -21,
            GwlId = -12
        }

        public enum Lwa
        {
            ColorKey = 0x1,
            Alpha = 0x2
        }

        [Flags]
        private enum WindowStyles : uint
        {
            WsExTransparent = 0x00000020,
            WsExLayered = 0x00080000
        }

        #endregion

        #region Constants

        private const int MY_LPARAM = 0x5073d; //used for maximizing the screen.
        private const int MY_W_PARAM = 0xf120; //used for maximizing the screen.
        private const int WM_SYSCOMMAND = 0x0112; //used for maximizing the screen.

        #endregion

        #region Fields

        private readonly Timer _drawBackgroundWorker = new Timer(200);
        private Rotation _lastSelectedRotation;

        private int _oldWindowLong;

        #endregion

        #region Constructors

        public Overlay()
        {
            InitializeComponent();

            MaximizeEverything();

            SetFormTransparent(Handle);

            SetTheLayeredWindowAttribute();

            _drawBackgroundWorker.Elapsed += DrawBackgroundWorkerOnElapsed;
            _drawBackgroundWorker.Start();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Finds the Size of all computer screens combined (assumes screens are left to right, not above and below).
        /// </summary>
        /// <returns>The width and height of all screens combined</returns>
        public static Size GetFullScreensSize()
        {
            return new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        }

        /// <summary>
        ///     Finds the top left pixel position (with multiple screens this is often not 0,0)
        /// </summary>
        /// <returns>Position of top left pixel</returns>
        public static Point GetTopLeft()
        {
            return new Point(Screen.PrimaryScreen.Bounds.Left, Screen.PrimaryScreen.Bounds.Top);
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, uint wMsg, UIntPtr wParam, IntPtr lParam); //used for maximizing the screen

        [DllImport("user32.dll")]
        private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        /// <summary>
        ///     Make the form (specified by its handle) a window that supports transparency.
        /// </summary>
        /// <param name="handle">The window to make transparency supporting</param>
        public void SetFormTransparent(IntPtr handle)
        {
            _oldWindowLong = GetWindowLong(handle, (int)GetWindowLongConst.GwlExstyle);
            SetWindowLong(handle, (int)GetWindowLongConst.GwlExstyle, Convert.ToInt32(_oldWindowLong | (uint)WindowStyles.WsExLayered | (uint)WindowStyles.WsExTransparent));
        }

        /// <summary>
        ///     Makes the form change White to Transparent and clickthrough-able
        ///     Can be modified to make the form translucent (with different opacities) and change the Transparency Color.
        /// </summary>
        public void SetTheLayeredWindowAttribute()
        {
            var transparentColor = 0xffffffff;

            SetLayeredWindowAttributes(Handle, transparentColor, 125, 0x2);

            TransparencyKey = Color.White;
        }

        private void DrawBackgroundWorkerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            var selectedRotation = MainWindowViewModel.StaticSelectedRotation;

            if (_lastSelectedRotation == selectedRotation) return;
            _lastSelectedRotation = selectedRotation;

            using (var graphics = CreateGraphics())
            {
                graphics.Clear(Color.White);

                if (selectedRotation == null) return;

                graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                graphics.DrawString(selectedRotation.Title, new Font("Calibri", 16, FontStyle.Italic), Brushes.WhiteSmoke, 20, Screen.PrimaryScreen.Bounds.Bottom - 30);
            }
        }

        private void MaximizeEverything()
        {
            Location = GetTopLeft();
            Size = GetFullScreensSize();

            SendMessage(Handle, WM_SYSCOMMAND, (UIntPtr)MY_W_PARAM, (IntPtr)MY_LPARAM);
        }

        #endregion
    }
}
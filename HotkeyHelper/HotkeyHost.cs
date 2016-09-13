using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace AutoGarrisonMissions.HotkeyHelper
{
    /// <summary>
    /// The HotkeyHost needed for working with hotKeys.
    /// </summary>
    public sealed class HotkeyHost : IDisposable
    {
        #region Fields

        private const int WM_HOT_KEY = 786;
        private static readonly SerialCounter _idGen = new SerialCounter(1);
        
        private readonly HwndSourceHook _hook;
        private readonly Dictionary<int, Hotkey> _hotKeys = new Dictionary<int, Hotkey>();
        private readonly HwndSource _hwndSource;
        private bool _disposed;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new HotkeyHost
        /// </summary>
        /// <param name="hwndSource">The handle of the window. Must not be null.</param>
        public HotkeyHost(HwndSource hwndSource)
        {
            if (hwndSource == null)
                throw new ArgumentNullException("hwndSource");

            _hook = WndProc;
            _hwndSource = hwndSource;
            hwndSource.AddHook(_hook);
        }

        #endregion

        #region Destructors

        ~HotkeyHost()
        {
            Dispose(false);
        }

        #endregion

        #region Events

        /// <summary>
        /// Will be raised if any registered Hotkey is pressed
        /// </summary>
        public event EventHandler<HotkeyEventArgs> HotKeyPressed;

        #endregion

        #region Properties

        /// <summary>
        /// All registered hotKeys
        /// </summary>
        public IEnumerable<Hotkey> HotKeys
        {
            get { return _hotKeys.Values; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds an Hotkey.
        /// </summary>
        /// <param name="hotkey">The Hotkey which will be added. Must not be null and can be registed only once.</param>
        public void AddHotKey(Hotkey hotkey)
        {
            if (hotkey == null)
                throw new ArgumentNullException();
            if (hotkey.Key == 0)
                throw new ArgumentNullException();
            if (_hotKeys.ContainsValue(hotkey))
                throw new HotkeyAlreadyRegisteredException("Hotkey already registered!", hotkey);

            var id = _idGen.Next();
            if (hotkey.Enabled)
                RegisterHotKey(id, hotkey);
            hotkey.PropertyChanged += hotKey_PropertyChanged;
            _hotKeys[id] = hotkey;
        }

        /// <summary>
        /// Removes an Hotkey
        /// </summary>
        /// <param name="hotkey">The Hotkey to be removed</param>
        /// <returns>True if success, otherwise false</returns>
        public bool RemoveHotKey(Hotkey hotkey)
        {
            var kvPair = _hotKeys.FirstOrDefault(h => h.Value.Equals(hotkey));
            if (kvPair.Value != null)
            {
                kvPair.Value.PropertyChanged -= hotKey_PropertyChanged;
                if (kvPair.Value.Enabled)
                    UnregisterHotKey(kvPair.Key);
                return _hotKeys.Remove(kvPair.Key);
            }
            return false;
        }

        #endregion

        #region Private Methods

        [DllImport("user32", CharSet = CharSet.Ansi,
            SetLastError = true, ExactSpelling = true)]
        private static extern int RegisterHotKey(IntPtr hwnd,
            int id, int modifiers, int key);

        [DllImport("user32", CharSet = CharSet.Ansi,
            SetLastError = true, ExactSpelling = true)]
        private static extern int UnregisterHotKey(IntPtr hwnd, int id);
        
        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _hwndSource.RemoveHook(_hook);
            }

            for (int i = _hotKeys.Count - 1; i >= 0; i--)
            {
                RemoveHotKey(_hotKeys.Values.ElementAt(i));
            }


            _disposed = true;
        }

        private void RegisterHotKey(int id, Hotkey hotkey)
        {
            if ((int) _hwndSource.Handle != 0)
            {
                RegisterHotKey(_hwndSource.Handle, id, (int) hotkey.Modifiers, KeyInterop.VirtualKeyFromKey(hotkey.Key));
                int error = Marshal.GetLastWin32Error();
                if (error != 0)
                {
                    Exception e = new Win32Exception(error);

                    if (error == 1409)
                        throw new HotkeyAlreadyRegisteredException(e.Message, hotkey, e);

                    throw e;
                }
            }
            else
                throw new InvalidOperationException("Handle is invalid");
        }

        private void UnregisterHotKey(int id)
        {
            if ((int) _hwndSource.Handle != 0)
            {
                UnregisterHotKey(_hwndSource.Handle, id);
                int error = Marshal.GetLastWin32Error();
                if (error != 0)
                    throw new Win32Exception(error);
            }
        }
        
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOT_KEY)
            {
                if (_hotKeys.ContainsKey((int) wParam))
                {
                    Hotkey h = _hotKeys[(int) wParam];
                    h.RaiseOnHotKeyPressed();
                    if (HotKeyPressed != null)
                        HotKeyPressed(this, new HotkeyEventArgs(h));
                }
            }

            return new IntPtr(0);
        }
        
        private void hotKey_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var kvPair = _hotKeys.FirstOrDefault(h => h.Value.Equals(sender));
            if (kvPair.Value != null)
            {
                if (e.PropertyName == "Enabled")
                {
                    if (kvPair.Value.Enabled)
                        RegisterHotKey(kvPair.Key, kvPair.Value);
                    else
                        UnregisterHotKey(kvPair.Key);
                }
                else if (e.PropertyName == "Key" || e.PropertyName == "Modifiers")
                {
                    if (kvPair.Value.Enabled)
                    {
                        UnregisterHotKey(kvPair.Key);
                        RegisterHotKey(kvPair.Key, kvPair.Value);
                    }
                }
            }
        }

        #endregion

        #region Types

        public class SerialCounter
        {
            #region Constructors

            public SerialCounter(int start)
            {
                Current = start;
            }

            #endregion

            #region Properties

            public int Current { get; private set; }

            #endregion

            #region Public Methods

            public int Next()
            {
                return ++Current;
            }

            #endregion
        }

        #endregion
    }
}
using System;

namespace RotationHelper.Helper.Hotkey
{
    public class HotkeyEventArgs : EventArgs
    {

        #region Constructors

        public HotkeyEventArgs(Hotkey hotkey)
        {
            Hotkey = hotkey;
        }

        #endregion

        #region Properties

        public Hotkey Hotkey { get; }

        #endregion

    }
}
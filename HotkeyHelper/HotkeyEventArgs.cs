using System;

namespace AutoGarrisonMissions.HotkeyHelper
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

        public Hotkey Hotkey { get; private set; }

        #endregion
    }
}
using System;
using System.Runtime.Serialization;

namespace RotationHelper.Helper.Hotkey
{
    [Serializable]
    public class HotkeyAlreadyRegisteredException : Exception
    {

        #region Constructors

        public HotkeyAlreadyRegisteredException(string message, Hotkey hotkey)
            : base(message)
        {
            Hotkey = hotkey;
        }

        public HotkeyAlreadyRegisteredException(string message, Hotkey hotkey, Exception inner)
            : base(message, inner)
        {
            Hotkey = hotkey;
        }

        protected HotkeyAlreadyRegisteredException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Properties

        public Hotkey Hotkey { get; }

        #endregion

    }
}
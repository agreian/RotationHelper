using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows.Input;

namespace RotationHelper.Helper.Hotkey
{
    /// <summary>
    ///     Represents an Hotkey
    /// </summary>
    [Serializable]
    public class Hotkey : INotifyPropertyChanged, ISerializable, IEquatable<Hotkey>
    {

        #region Events

        /// <summary>
        ///     Will be raised if the hotkey is pressed (works only if registed in HotkeyHost)
        /// </summary>
        public event EventHandler<HotkeyEventArgs> HotKeyPressed;

        #endregion

        #region Fields

        private bool _enabled;
        private Key _key;
        private ModifierKeys _modifiers;

        #endregion

        #region Constructors

        /// <summary>
        ///     Creates an Hotkey object. This instance has to be registered in an HotkeyHost.
        /// </summary>
        public Hotkey()
        {
        }

        /// <summary>
        ///     Creates an Hotkey object. This instance has to be registered in an HotkeyHost.
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="modifiers">The modifier. Multiple modifiers can be combined with or.</param>
        public Hotkey(Key key, ModifierKeys modifiers)
            : this(key, modifiers, true)
        {
        }

        /// <summary>
        ///     Creates an Hotkey object. This instance has to be registered in an HotkeyHost.
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="modifiers">The modifier. Multiple modifiers can be combined with or.</param>
        /// <param name="enabled">Specifies whether the Hotkey will be enabled when registered to an HotkeyHost</param>
        public Hotkey(Key key, ModifierKeys modifiers, bool enabled)
        {
            Key = key;
            Modifiers = modifiers;
            Enabled = enabled;
        }

        protected Hotkey(SerializationInfo info, StreamingContext context)
        {
            Key = (Key)info.GetValue("Key", typeof(Key));
            Modifiers = (ModifierKeys)info.GetValue("Modifiers", typeof(ModifierKeys));
            Enabled = info.GetBoolean("Enabled");
        }

        #endregion

        #region Properties

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (value != _enabled)
                {
                    _enabled = value;
                    RaisePropertyChanged("Enabled");
                }
            }
        }

        /// <summary>
        ///     The Key. Must not be null when registering to an HotkeyHost.
        /// </summary>
        public Key Key
        {
            get => _key;
            set
            {
                if (_key != value)
                {
                    _key = value;
                    RaisePropertyChanged("Key");
                }
            }
        }

        /// <summary>
        ///     The modifier. Multiple modifiers can be combined with or.
        /// </summary>
        public ModifierKeys Modifiers
        {
            get => _modifiers;
            set
            {
                if (_modifiers != value)
                {
                    _modifiers = value;
                    RaisePropertyChanged("Modifiers");
                }
            }
        }

        #endregion

        #region IEquatable<Hotkey> Members

        public bool Equals(Hotkey other)
        {
            return other != null && Key == other.Key && Modifiers == other.Modifiers;
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region ISerializable Members

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Key", Key, typeof(Key));
            info.AddValue("Modifiers", Modifiers, typeof(ModifierKeys));
            info.AddValue("Enabled", Enabled);
        }

        #endregion

        #region Methods

        #region Public Methods

        public override bool Equals(object obj)
        {
            var hotkey = obj as Hotkey;
            return hotkey != null && Equals(hotkey);
        }

        public override int GetHashCode()
        {
            return (int)Modifiers + 10 * (int)Key;
        }

        public override string ToString()
        {
            return $"{Key} + {Modifiers} ({(Enabled ? "" : "Not ")}Enabled)";
        }

        #endregion

        #region Internal Methods

        internal void RaiseOnHotKeyPressed()
        {
            RaiseHotKeyPress();
        }

        #endregion

        #region Protected Methods

        protected virtual void RaiseHotKeyPress()
        {
            HotKeyPressed?.Invoke(this, new HotkeyEventArgs(this));
        }

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #endregion

    }
}
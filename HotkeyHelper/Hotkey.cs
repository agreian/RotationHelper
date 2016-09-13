using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows.Input;

namespace AutoGarrisonMissions.HotkeyHelper
{
    /// <summary>
    /// Represents an Hotkey
    /// </summary>
    [Serializable]
    public class Hotkey : INotifyPropertyChanged, ISerializable, IEquatable<Hotkey>
    {
        #region Fields

        private bool _enabled;
        private Key _key;
        private ModifierKeys _modifiers;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an Hotkey object. This instance has to be registered in an HotkeyHost.
        /// </summary>
        public Hotkey()
        {
        }

        /// <summary>
        /// Creates an Hotkey object. This instance has to be registered in an HotkeyHost.
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="modifiers">The modifier. Multiple modifiers can be combined with or.</param>
        public Hotkey(Key key, ModifierKeys modifiers) : this(key, modifiers, true)
        {
        }

        /// <summary>
        /// Creates an Hotkey object. This instance has to be registered in an HotkeyHost.
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
            Key = (Key) info.GetValue("Key", typeof (Key));
            Modifiers = (ModifierKeys) info.GetValue("Modifiers", typeof (ModifierKeys));
            Enabled = info.GetBoolean("Enabled");
        }

        #endregion

        #region Events

        /// <summary>
        /// Will be raised if the hotkey is pressed (works only if registed in HotkeyHost)
        /// </summary>
        public event EventHandler<HotkeyEventArgs> HotKeyPressed;

        #endregion

        #region Properties

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (value != _enabled)
                {
                    _enabled = value;
                    OnPropertyChanged("Enabled");
                }
            }
        }


        /// <summary>
        /// The Key. Must not be null when registering to an HotkeyHost.
        /// </summary>
        public Key Key
        {
            get { return _key; }
            set
            {
                if (_key != value)
                {
                    _key = value;
                    OnPropertyChanged("Key");
                }
            }
        }

        /// <summary>
        /// The modifier. Multiple modifiers can be combined with or.
        /// </summary>
        public ModifierKeys Modifiers
        {
            get { return _modifiers; }
            set
            {
                if (_modifiers != value)
                {
                    _modifiers = value;
                    OnPropertyChanged("Modifiers");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region ISerializable Members

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Key", Key, typeof (Key));
            info.AddValue("Modifiers", Modifiers, typeof (ModifierKeys));
            info.AddValue("Enabled", Enabled);
        }

        #endregion

        #region IEquatable<Hotkey> Members

        public bool Equals(Hotkey other)
        {
            return (Key == other.Key && Modifiers == other.Modifiers);
        }

        #endregion

        #region Public Methods

        public override bool Equals(object obj)
        {
            var hotkey = obj as Hotkey;
            return hotkey != null && Equals(hotkey);
        }

        public override int GetHashCode()
        {
            return (int) Modifiers + 10*(int) Key;
        }

        public override string ToString()
        {
            return string.Format("{0} + {1} ({2}Enabled)", Key, Modifiers, Enabled ? "" : "Not ");
        }

        #endregion

        #region Private Methods

        internal void RaiseOnHotKeyPressed()
        {
            OnHotKeyPress();
        }

        protected virtual void OnHotKeyPress()
        {
            if (HotKeyPressed != null)
                HotKeyPressed(this, new HotkeyEventArgs(this));
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
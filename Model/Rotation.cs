using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RotationHelper.Model
{
    [Serializable]
    public class Rotation : INotifyPropertyChanged
    {

        #region Fields

        private string _title;

        #endregion

        #region Constructors

        public Rotation()
        {
            KeyCommands = new ObservableCollection<KeyCommand>();
        }

        #endregion

        #region Properties

        public ObservableCollection<KeyCommand> KeyCommands { get; }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Methods

        #region Protected Methods

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #endregion

    }
}
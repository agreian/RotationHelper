using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace RotationHelper
{
    public class EditRotationViewModel : ViewModelBase
    {
        #region Fields

        private KeyCommand _selectedKeyCommand;
        private Rotation _selectedRotation;

        #endregion

        #region Constructors

        public EditRotationViewModel(RotationHelperFile rotationHelperFile)
        {
            RotationHelperFile = rotationHelperFile;

            AddRotationCommand = new RelayCommand(AddRotationAction);
            RemoveRotationCommand = new RelayCommand(RemoveRotationAction);
            AddKeyCommand = new RelayCommand(AddKeyAction);
            RemoveKeyCommand = new RelayCommand(RemoveKeyAction);

            if (RotationHelperFile.Rotations.Count == 0) AddRotationAction();
            SelectedRotation = RotationHelperFile.Rotations.First();
        }

        #endregion

        #region Properties

        public RelayCommand AddKeyCommand { get; }

        public RelayCommand AddRotationCommand { get; }

        public RelayCommand RemoveKeyCommand { get; }

        public RelayCommand RemoveRotationCommand { get; }

        public RotationHelperFile RotationHelperFile { get; }

        public KeyCommand SelectedKeyCommand
        {
            get { return _selectedKeyCommand; }
            set
            {
                _selectedKeyCommand = value;
                RaisePropertyChanged(() => SelectedKeyCommand);
            }
        }

        public Rotation SelectedRotation
        {
            get { return _selectedRotation; }
            set
            {
                _selectedRotation = value;
                RaisePropertyChanged(() => SelectedRotation);
            }
        }

        #endregion

        #region Methods

        private void AddKeyAction()
        {
            if (SelectedRotation == null) return;

            SelectedRotation.KeyCommands.Add(new KeyCommand());

            SelectedRotation.KeyCommands.Last().Key = KeyCommand.PossibleKeys.First();
        }

        private void AddRotationAction()
        {
            RotationHelperFile.Rotations.Add(new Rotation { Title = $"Rotation {RotationHelperFile.Rotations.Count + 1}" });

            if (SelectedRotation == null) SelectedRotation = RotationHelperFile.Rotations.First();
        }

        private void RemoveKeyAction()
        {
            if (SelectedKeyCommand == null) return;

            SelectedRotation?.KeyCommands.Remove(SelectedKeyCommand);
        }

        private void RemoveRotationAction()
        {
            if (SelectedRotation == null) return;

            RotationHelperFile.Rotations.Remove(SelectedRotation);
        }

        #endregion
    }
}
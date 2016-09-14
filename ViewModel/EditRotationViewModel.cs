using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Timers;
using System.Windows.Forms;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using RotationHelper.Helper;
using RotationHelper.Model;
using RotationHelper.View;
using Application = System.Windows.Application;
using Color = System.Windows.Media.Color;
using Timer = System.Timers.Timer;

namespace RotationHelper.ViewModel
{
    public class EditRotationViewModel : ViewModelBase
    {
        #region Fields

        private readonly Timer _mousePositionColorTimer = new Timer();

        private Point _mousePosition;
        private Color _mousePositionColor;
        private KeyCommand _selectedKeyCommand;
        private Rotation _selectedRotation;

        #endregion

        #region Constructors

        public EditRotationViewModel(EditRotationWindow editRotationWindow, RotationHelperFile rotationHelperFile)
        {
            RotationHelperFile = rotationHelperFile;

            AddRotationCommand = new RelayCommand(AddRotationAction);
            RemoveRotationCommand = new RelayCommand(RemoveRotationAction);
            AddKeyCommand = new RelayCommand(AddKeyAction);
            RemoveKeyCommand = new RelayCommand(RemoveKeyAction);

            if (RotationHelperFile.Rotations.Count == 0) AddRotationAction();
            SelectedRotation = RotationHelperFile.Rotations.First();

            _mousePositionColorTimer.Interval = 200;
            _mousePositionColorTimer.Elapsed += MousePositionColorTimerOnElapsed;
            _mousePositionColorTimer.Start();

            editRotationWindow.Closing += EditRotationWindowOnClosing;
        }

        #endregion

        #region Properties

        public RelayCommand AddKeyCommand { get; }

        public RelayCommand AddRotationCommand { get; }

        public string MousePixelColor => $"Red: {_mousePositionColor.R}, Green: {_mousePositionColor.G}, Blue: {_mousePositionColor.B} at ({_mousePosition.X}:{_mousePosition.Y})";

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

        private void EditRotationWindowOnClosing(object sender, CancelEventArgs e)
        {
            _mousePositionColorTimer.Stop();
            _mousePositionColorTimer.Close();
            _mousePositionColorTimer.Dispose();
        }

        private void MousePositionColorTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            _mousePosition = Cursor.Position;
            _mousePositionColor = ScreenshotHelper.GetColorAt(_mousePosition).First();

            Application.Current.Dispatcher.BeginInvoke((Action)(() => RaisePropertyChanged(() => MousePixelColor)));
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
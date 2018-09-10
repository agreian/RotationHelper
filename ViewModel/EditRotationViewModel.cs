using System;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

using RotationHelper.Helper;
using RotationHelper.Helper.Hotkey;
using RotationHelper.Model;
using RotationHelper.View;

using Color = System.Windows.Media.Color;
using Cursor = System.Windows.Forms.Cursor;
using Point = System.Drawing.Point;

namespace RotationHelper.ViewModel
{
    public class EditRotationViewModel : ViewModelBase
    {

        #region Fields

        private readonly Timer _mousePositionColorTimer = new Timer();

        private Hotkey _copyMousePositionColorHotkey;
        private HotkeyHost _hotkeyHost;
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
            MoveUpCommand = new RelayCommand(MoveUpAction);
            MoveDownCommand = new RelayCommand(MoveDownAction);

            if (RotationHelperFile.Rotations.Count == 0) AddRotationAction();
            SelectedRotation = RotationHelperFile.Rotations.First();

            _mousePositionColorTimer.Interval = 200;
            _mousePositionColorTimer.Elapsed += MousePositionColorTimerOnElapsed;
            _mousePositionColorTimer.Start();

            editRotationWindow.Closing += EditRotationWindowOnClosing;
            editRotationWindow.Loaded += EditRotationWindowOnLoaded;
            editRotationWindow.Unloaded += EditRotationWindowOnUnloaded;
        }

        #endregion

        #region Properties

        public RelayCommand AddKeyCommand { get; }

        public RelayCommand AddRotationCommand { get; }

        public string MousePixelColor => $"Red: {_mousePositionColor.R}, Green: {_mousePositionColor.G}, Blue: {_mousePositionColor.B} at ({_mousePosition.X}:{_mousePosition.Y})";

        public RelayCommand MoveDownCommand { get; }

        public RelayCommand MoveUpCommand { get; }

        public RelayCommand RemoveKeyCommand { get; }

        public RelayCommand RemoveRotationCommand { get; }

        public RotationHelperFile RotationHelperFile { get; }

        public KeyCommand SelectedKeyCommand
        {
            get => _selectedKeyCommand;
            set
            {
                _selectedKeyCommand = value;
                RaisePropertyChanged(() => SelectedKeyCommand);
            }
        }

        public Rotation SelectedRotation
        {
            get => _selectedRotation;
            set
            {
                _selectedRotation = value;
                RaisePropertyChanged(() => SelectedRotation);
            }
        }

        #endregion

        #region Methods

        #region Private Methods

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

        private void EditRotationWindowOnLoaded(object sender, RoutedEventArgs e)
        {
            _hotkeyHost = new HotkeyHost((HwndSource)PresentationSource.FromVisual(Application.Current.MainWindow));

            _copyMousePositionColorHotkey = new Hotkey(Key.C, ModifierKeys.Control);
            _copyMousePositionColorHotkey.HotKeyPressed += OnCopyMousePositionColorHotKeyPressed;

            _hotkeyHost.AddHotKey(_copyMousePositionColorHotkey);
        }

        private void EditRotationWindowOnUnloaded(object sender, RoutedEventArgs e)
        {
            _hotkeyHost.RemoveHotKey(_copyMousePositionColorHotkey);
        }

        private void MousePositionColorTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            _mousePosition = Cursor.Position;
            _mousePositionColor = ScreenshotHelper.GetColorAt(_mousePosition).First();

            Application.Current.Dispatcher.BeginInvoke((Action)(() => RaisePropertyChanged(() => MousePixelColor)));
        }

        private void MoveDownAction()
        {
            if (SelectedKeyCommand == null || SelectedRotation == null) return;

            var oldIndex = SelectedRotation.KeyCommands.IndexOf(SelectedKeyCommand);
            var newIndex = oldIndex + 1;

            if (newIndex >= SelectedRotation.KeyCommands.Count) return;

            SelectedRotation.KeyCommands.Move(oldIndex, newIndex);
        }

        private void MoveUpAction()
        {
            if (SelectedKeyCommand == null || SelectedRotation == null) return;

            var oldIndex = SelectedRotation.KeyCommands.IndexOf(SelectedKeyCommand);
            var newIndex = oldIndex - 1;

            if (newIndex < 0) return;

            SelectedRotation.KeyCommands.Move(oldIndex, newIndex);
        }

        private void OnCopyMousePositionColorHotKeyPressed(object sender, HotkeyEventArgs e)
        {
            if (SelectedKeyCommand == null) return;

            SelectedKeyCommand.X = _mousePosition.X;
            SelectedKeyCommand.Y = _mousePosition.Y;
            SelectedKeyCommand.Red = _mousePositionColor.R;
            SelectedKeyCommand.Green = _mousePositionColor.G;
            SelectedKeyCommand.Blue = _mousePositionColor.B;
        }

        private void RemoveKeyAction()
        {
            if (SelectedKeyCommand == null || SelectedRotation == null) return;

            var oldIndex = SelectedRotation.KeyCommands.IndexOf(SelectedKeyCommand);

            SelectedRotation.KeyCommands.Remove(SelectedKeyCommand);
            if (SelectedRotation.KeyCommands.Count > 0) SelectedKeyCommand = SelectedRotation.KeyCommands.ElementAt(oldIndex >= SelectedRotation.KeyCommands.Count ? oldIndex - 1 : oldIndex);
        }

        private void RemoveRotationAction()
        {
            if (SelectedRotation == null) return;

            RotationHelperFile.Rotations.Remove(SelectedRotation);
        }

        #endregion

        #endregion

    }
}
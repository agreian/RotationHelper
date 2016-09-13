using AutoGarrisonMissions.HotkeyHelper;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using WindowsInput;
using WindowsInput.Native;

namespace RotationHelper
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Constants

        private const string DefaultExt = ".rotation";
        private const string RotationHelperFilesRotationRotation = "Rotation Helper files (*.rotation)|*.rotation";

        #endregion

        #region Fields

        private readonly Random _randomKeyPress = new Random();
        private readonly Random _randomTimer = new Random();
        private readonly Semaphore _semaphore = new Semaphore(1, 1);
        private readonly System.Timers.Timer _rotationTimer;

        private Hotkey _controlLHotkey;
        private Hotkey _controlPHotkey;
        private HotkeyHost _hotkeyHost;
        private bool _isStarted;
        private string _loadedFilePath;
        private Rotation _selectedRotation;

        #endregion

        #region Constructors

        public MainWindowViewModel(MainWindow mainWindow)
        {
            MainWindow = mainWindow;
            LoggingTextBox = mainWindow.ScrollingTextBox;

            NewCommand = new RelayCommand(NewAction);
            LoadCommand = new RelayCommand(LoadAction);
            SaveCommand = new RelayCommand(SaveAction);
            EditCommand = new RelayCommand(EditAction);
            StartStopCommand = new RelayCommand(StartStopAction);

            InputSimulator = new InputSimulator();
            KeyboardSimulator = new KeyboardSimulator(InputSimulator);

            MainWindow.Loaded += MainWindowOnLoaded;
            MainWindow.Unloaded += MainWindowOnUnloaded;

            _rotationTimer = new System.Timers.Timer(160);
            _rotationTimer.Elapsed += RotationTimerCallback;
        }

        #endregion

        #region Properties

        public RotationHelperFile CurrentRotationHelperFile { get; set; }

        public RelayCommand EditCommand { get; }

        public InputSimulator InputSimulator { get; }

        public bool IsStarted
        {
            get { return _isStarted; }
            set
            {
                _isStarted = value;
                RaisePropertyChanged(() => StartStopContent);
            }
        }

        public KeyboardSimulator KeyboardSimulator { get; }

        public RelayCommand LoadCommand { get; }

        public string LoadedFileName => string.IsNullOrWhiteSpace(LoadedFilePath) ? null : Path.GetFileNameWithoutExtension(LoadedFilePath);

        public string LoadedFilePath
        {
            get { return _loadedFilePath; }
            set
            {
                _loadedFilePath = value;
                RaisePropertyChanged(() => LoadedFileName);
            }
        }

        public ScrollingTextBox LoggingTextBox { get; }

        public MainWindow MainWindow { get; }

        public RelayCommand NewCommand { get; }

        public RelayCommand SaveCommand { get; }

        public Rotation SelectedRotation
        {
            get { return _selectedRotation; }
            set
            {
                _selectedRotation = value;
                RaisePropertyChanged(() => SelectedRotation);
            }
        }

        public RelayCommand StartStopCommand { get; }

        public string StartStopContent => IsStarted ? "Stop" : "Start";

        #endregion

        #region Methods

        public void KeyPress(VirtualKeyCode keyCode)
        {
            KeyboardSimulator.KeyPress(keyCode);
        }

        private void EditAction()
        {
            if (CurrentRotationHelperFile == null || IsStarted) return;

            var editWindow = new EditRotationWindow(CurrentRotationHelperFile);
            editWindow.ShowDialog();
        }

        private void LoadAction()
        {
            if (IsStarted) return;

            var dlg = new OpenFileDialog { DefaultExt = DefaultExt, Multiselect = false, Filter = RotationHelperFilesRotationRotation };

            var result = dlg.ShowDialog();
            if (result != true) return;

            LoadFile(dlg.FileName);
        }

        private void LoadFile(string fileName)
        {
            CurrentRotationHelperFile = RotationHelperFile.Deserialize(fileName);

            LoadedFilePath = fileName;
        }

        private void MainWindowOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            _hotkeyHost = new HotkeyHost((HwndSource)PresentationSource.FromVisual(Application.Current.MainWindow));
            _controlPHotkey = new Hotkey(Key.P, ModifierKeys.Control);
            _controlPHotkey.HotKeyPressed += OnControlPHotkeyPressed;

            _controlLHotkey = new Hotkey(Key.L, ModifierKeys.Control);
            _controlLHotkey.HotKeyPressed += OnControlLHotkeyPressed;

            _hotkeyHost.AddHotKey(_controlPHotkey);
            _hotkeyHost.AddHotKey(_controlLHotkey);
        }

        private void MainWindowOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (IsStarted) StartStopAction();

            _hotkeyHost.RemoveHotKey(_controlPHotkey);
            _hotkeyHost.RemoveHotKey(_controlLHotkey);
        }

        private void NewAction()
        {
            if (IsStarted) return;

            CurrentRotationHelperFile = new RotationHelperFile();

            LoadedFilePath = null;
        }

        private void OnControlLHotkeyPressed(object sender, HotkeyEventArgs e)
        {
            if (CurrentRotationHelperFile == null || CurrentRotationHelperFile.Rotations.Count == 0 || SelectedRotation == null) return;

            var index = CurrentRotationHelperFile.Rotations.IndexOf(SelectedRotation);
            index++;
            SelectedRotation = index >= CurrentRotationHelperFile.Rotations.Count || index == -1 ? CurrentRotationHelperFile.Rotations.First() : CurrentRotationHelperFile.Rotations[index];
        }

        private void OnControlPHotkeyPressed(object sender, HotkeyEventArgs e)
        {
            StartStopAction();
        }

        private void RotationTimerCallback(object sender, System.Timers.ElapsedEventArgs e)
        {
            _semaphore.WaitOne();

            if (!IsStarted) return;

            _rotationTimer.Stop();

            var points = SelectedRotation.KeyCommands.Select(x => new System.Drawing.Point(x.X, x.Y)).Distinct().ToArray();

            var colors = ScreenshotHelper.GetColorAt(points);

            foreach (var color in colors)
            {
                var keys = SelectedRotation.KeyCommands.Where(x => x.Color.AreEquivalent(color)).ToArray();
                if (keys.Length == 0)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() => LoggingTextBox.AppendText($"Color {color} detected, no key attached{Environment.NewLine}")));
                }

                foreach (var key in keys)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() => LoggingTextBox.AppendText($"Color {color} detected, pressing {key.Key}{Environment.NewLine}")));
                    KeyPress(key.Key);
                }
            }

            _rotationTimer.Start();

            _semaphore.Release();
        }

        private void SaveAction()
        {
            if (CurrentRotationHelperFile == null) return;

            var path = LoadedFilePath;
            if (string.IsNullOrWhiteSpace(path))
            {
                var dlg = new SaveFileDialog { DefaultExt = DefaultExt, Filter = RotationHelperFilesRotationRotation };

                var result = dlg.ShowDialog();
                if (result != true) return;

                path = dlg.FileName;
            }

            CurrentRotationHelperFile.Serialize(path);

            LoadedFilePath = path;
        }

        private void StartStopAction()
        {
            if (CurrentRotationHelperFile == null || CurrentRotationHelperFile.Rotations.Count == 0) return;

            _semaphore.WaitOne();

            IsStarted = !IsStarted;

            if (IsStarted) // Stop
            {
                SelectedRotation = CurrentRotationHelperFile.Rotations.First();
                _rotationTimer.Start();
            }
            else
            {
                SelectedRotation = null;
                _rotationTimer.Stop();
            }

            _semaphore.Release();
        }

        #endregion
    }
}
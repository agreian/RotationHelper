using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using WindowsInput;
using WindowsInput.Native;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using RotationHelper.Helper;
using RotationHelper.Helper.Hotkey;
using RotationHelper.Model;
using RotationHelper.View;
using Point = System.Drawing.Point;
using Timer = System.Timers.Timer;

namespace RotationHelper.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Constants

        private const string DEFAULT_EXT = ".rotation";
        private const string ROTATION_HELPER_FILES_ROTATION_ROTATION = "Rotation Helper files (*.rotation)|*.rotation";

        #endregion

        #region Fields

        private readonly Timer _rotationTimer = new Timer(160);
        private readonly Semaphore _semaphore = new Semaphore(1, 1);
        private readonly Random _timerRandom = new Random();

        private Hotkey _changeRotationHotkey;
        private HotkeyHost _hotkeyHost;
        private bool _isStarted;
        private byte[] _loadedFileByteArray;
        private string _loadedFilePath;
        private Overlay _overlay;
        private Hotkey _startStopHotkey;

        #endregion

        #region Constructors

        public MainWindowViewModel(MainWindow mainWindow)
        {
            MainWindow = mainWindow;
            LoggingTextBox = mainWindow.ScrollingTextBox;

            NewCommand = new RelayCommand(NewAction, NewCanAction);
            LoadCommand = new RelayCommand(LoadAction, LoadCanAction);
            SaveCommand = new RelayCommand(SaveAction, SaveCanAction);
            EditCommand = new RelayCommand(EditAction, EditCanAction);
            StartStopCommand = new RelayCommand(StartStopAction, StartStopCanAction);

            SaveCommand.CanExecuteChanged += SaveCommandOnCanExecuteChanged;

            InputSimulator = new InputSimulator();
            KeyboardSimulator = new KeyboardSimulator(InputSimulator);

            MainWindow.Loaded += MainWindowOnLoaded;
            MainWindow.Closing += MainWindowOnClosing;
            MainWindow.Unloaded += MainWindowOnUnloaded;

            _rotationTimer.Elapsed += RotationTimerOnElapsed;

            if (App.FileToOpen != null && File.Exists(App.FileToOpen)) LoadFile(App.FileToOpen);
        }

        #endregion

        #region Properties

        public static Rotation StaticSelectedRotation { get; private set; }

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

                if (LoadedFilePath != null && File.Exists(LoadedFilePath))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var fileStream2 = File.Open(LoadedFilePath, FileMode.Open))
                        {
                            fileStream2.CopyTo(memoryStream);
                        }
                        _loadedFileByteArray = memoryStream.ToArray();
                    }
                }
                else _loadedFileByteArray = null;
            }
        }

        public ScrollingTextBox LoggingTextBox { get; }

        public MainWindow MainWindow { get; }

        public RelayCommand NewCommand { get; }

        public RelayCommand SaveCommand { get; }

        public Rotation SelectedRotation
        {
            get { return StaticSelectedRotation; }
            set
            {
                StaticSelectedRotation = value;
                RaisePropertyChanged(() => SelectedRotation);
            }
        }

        public RelayCommand StartStopCommand { get; }

        public string StartStopContent => IsStarted ? "Stop" : "Start";

        public string WindowTitle => $"Rotation Helper{(LoadedFilePath == null ? "" : $" - {LoadedFileName}{(IsSaveNeeded() ? "*" : "")}")}";

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

            SelectedRotation = CurrentRotationHelperFile.Rotations.FirstOrDefault();
        }

        private bool EditCanAction()
        {
            return CurrentRotationHelperFile != null && IsStarted == false;
        }

        private bool IsSaveNeeded()
        {
            if (CurrentRotationHelperFile == null || _loadedFileByteArray == null) return true;

            try
            {
                byte[] byteArray;

                using (var memoryStream = new MemoryStream())
                {
                    CurrentRotationHelperFile.Serialize(memoryStream);
                    byteArray = memoryStream.ToArray();
                }

                if (byteArray.Length != _loadedFileByteArray.Length) return true;

                // ReSharper disable once LoopCanBeConvertedToQuery
                for (var i = 0; i < byteArray.Length; ++i) if (byteArray[i] != _loadedFileByteArray[i]) return true;

                return false;
            }
            catch
            {
                return true;
            }
        }

        private void LoadAction()
        {
            if (IsStarted) return;

            var dlg = new OpenFileDialog { DefaultExt = DEFAULT_EXT, Multiselect = false, Filter = ROTATION_HELPER_FILES_ROTATION_ROTATION };

            var result = dlg.ShowDialog();
            if (result != true || File.Exists(dlg.FileName) == false) return;

            LoadFile(dlg.FileName);
        }

        private bool LoadCanAction()
        {
            return IsStarted == false;
        }

        private void LoadFile(string fileName)
        {
            if (File.Exists(fileName) == false) return;

            CurrentRotationHelperFile = RotationHelperFile.Deserialize(fileName);
            LoadedFilePath = fileName;
            App.AddRecentFile(fileName);
            SelectedRotation = CurrentRotationHelperFile.Rotations.FirstOrDefault();
        }

        private void MainWindowOnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            if (IsStarted) StartStopAction();

            if (LoadedFilePath == null || IsSaveNeeded() == false) return;

            var result = MessageBox.Show("Your modifications will be lost, do you want to save ?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes) SaveAction();
        }

        private void MainWindowOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            _hotkeyHost = new HotkeyHost((HwndSource)PresentationSource.FromVisual(Application.Current.MainWindow));
            _startStopHotkey = new Hotkey(Key.P, ModifierKeys.Control);
            _startStopHotkey.HotKeyPressed += OnStartStopHotkeyPressed;

            _changeRotationHotkey = new Hotkey(Key.L, ModifierKeys.Control);
            _changeRotationHotkey.HotKeyPressed += OnChangeRotationHotkeyPressed;

            _hotkeyHost.AddHotKey(_startStopHotkey);
            _hotkeyHost.AddHotKey(_changeRotationHotkey);
        }

        private void MainWindowOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            _hotkeyHost.RemoveHotKey(_startStopHotkey);
            _hotkeyHost.RemoveHotKey(_changeRotationHotkey);

            _rotationTimer.Stop();
            _rotationTimer.Close();
            _rotationTimer.Dispose();
        }

        private void NewAction()
        {
            if (IsStarted) return;

            CurrentRotationHelperFile = new RotationHelperFile();

            LoadedFilePath = null;

            SaveAction();

            if (LoadedFilePath == null) CurrentRotationHelperFile = null;
        }

        private bool NewCanAction()
        {
            return IsStarted == false;
        }

        private void OnChangeRotationHotkeyPressed(object sender, HotkeyEventArgs e)
        {
            if (CurrentRotationHelperFile == null || CurrentRotationHelperFile.Rotations.Count == 0) return;

            var index = CurrentRotationHelperFile.Rotations.IndexOf(SelectedRotation);
            index++;
            SelectedRotation = index >= CurrentRotationHelperFile.Rotations.Count || index == -1 ? CurrentRotationHelperFile.Rotations.First() : CurrentRotationHelperFile.Rotations[index];

            SystemSounds.Asterisk.Play();
        }

        private void OnStartStopHotkeyPressed(object sender, HotkeyEventArgs e)
        {
            StartStopAction();

            SystemSounds.Beep.Play();
        }

        private void RotationTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            _semaphore.WaitOne();

            if (!IsStarted) return;

            _rotationTimer.Stop();

            Thread.Sleep(_timerRandom.Next(0, 81));

            if (SelectedRotation.KeyCommands.Count == 0) Application.Current.Dispatcher.BeginInvoke((Action)(() => LoggingTextBox.AppendText($"Selected rotation has no configured key, please edit it!{Environment.NewLine}")));

            var points = SelectedRotation.KeyCommands.Select(x => new Point(x.X, x.Y)).Distinct().ToArray();

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
                    Application.Current.Dispatcher.BeginInvoke((Action)(() => LoggingTextBox.AppendText($"Color {color} detected, pressing {key.Key} ({key.Name}){Environment.NewLine}")));
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
                var dlg = new SaveFileDialog { DefaultExt = DEFAULT_EXT, Filter = ROTATION_HELPER_FILES_ROTATION_ROTATION };

                var result = dlg.ShowDialog();
                if (result != true) return;

                path = dlg.FileName;
            }

            CurrentRotationHelperFile.Serialize(path);
            App.AddRecentFile(path);

            LoadedFilePath = path;
        }

        private bool SaveCanAction()
        {
            return CurrentRotationHelperFile != null && IsStarted == false && IsSaveNeeded();
        }

        private void SaveCommandOnCanExecuteChanged(object sender, EventArgs eventArgs)
        {
            RaisePropertyChanged(() => WindowTitle);
        }

        private void StartStopAction()
        {
            if (CurrentRotationHelperFile == null || CurrentRotationHelperFile.Rotations.Count == 0) return;

            _semaphore.WaitOne();

            IsStarted = !IsStarted;

            if (IsStarted) // Stop
            {
                _rotationTimer.Start();
                _overlay = new Overlay();
                _overlay.Show();
            }
            else
            {
                _rotationTimer.Stop();
                _overlay.Close();
                _overlay = null;
            }

            _semaphore.Release();
        }

        private bool StartStopCanAction()
        {
            return CurrentRotationHelperFile != null && CurrentRotationHelperFile.Rotations.Count > 0;
        }

        #endregion
    }
}
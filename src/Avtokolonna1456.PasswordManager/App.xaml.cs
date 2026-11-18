using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Avtokolonna1456.PasswordManager.Services;
using Avtokolonna1456.PasswordManager.ViewModels;
using Avtokolonna1456.PasswordManager.Views;

namespace Avtokolonna1456.PasswordManager
{
    public partial class App : Application
    {
        private StorageService _storage;
        private BackupService _backup;
        private ClipboardService _clipboard;
        private ThemeService _theme;
        private AutostartService _autostart;
        private NotificationService _notifications;
        private TrayService _tray;
        private MainWindow _mainWindow;
        private MainViewModel _mainViewModel;
        private DispatcherTimer _timer;
        private bool _startMinimized;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _startMinimized = e.Args.Any(a =>
                string.Equals(a, "--minimized", StringComparison.OrdinalIgnoreCase));

            _storage = new StorageService();
            _backup = new BackupService(_storage);
            _clipboard = new ClipboardService();
            _theme = new ThemeService();
            _autostart = new AutostartService();
            _tray = new TrayService();
            _notifications = new NotificationService(text => _tray.ShowBalloon(text));

            Models.AppData data;
            try
            {
                data = _storage.Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка данных", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            var pinWindow = new PinWindow(data, _storage, new PinService());
            var pinResult = pinWindow.ShowDialog();
            if (pinResult != true)
            {
                Shutdown();
                return;
            }

            _mainViewModel = new MainViewModel(
                data, _storage, _backup, _clipboard, _theme, _autostart, _notifications);

            _mainWindow = new MainWindow(_mainViewModel);
            MainWindow = _mainWindow;

            _tray.Initialize();
            _tray.OpenRequested += ShowMainWindow;
            _tray.CheckRemindersRequested += () =>
            {
                ShowMainWindow();
                _mainViewModel.CheckSystemNotifications();
                _mainViewModel.ShowStartupReminders();
            };
            _tray.ExitRequested += ExitApp;

            _mainWindow.Closing += MainWindow_Closing;

            if (_startMinimized)
            {
                _mainWindow.Hide();
            }
            else
            {
                _mainWindow.Show();
                _mainViewModel.ShowStartupReminders();
            }

            _mainViewModel.CheckSystemNotifications();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(15)
            };
            _timer.Tick += (s, args) =>
            {
                _mainViewModel.RefreshAll();
                _mainViewModel.CheckSystemNotifications();
            };
            _timer.Start();

            if (data.AutostartEnabled && !_autostart.IsEnabled())
            {
                _autostart.SetEnabled(true);
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            _mainWindow.Hide();
        }

        private void ShowMainWindow()
        {
            if (_mainWindow == null) return;
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
        }

        private void ExitApp()
        {
            _timer?.Stop();
            if (_mainWindow != null)
            {
                _mainWindow.Closing -= MainWindow_Closing;
            }
            _tray?.Dispose();
            Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _timer?.Stop();
            _tray?.Dispose();
            base.OnExit(e);
        }
    }
}

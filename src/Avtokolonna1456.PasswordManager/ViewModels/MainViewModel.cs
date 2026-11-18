using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Avtokolonna1456.PasswordManager.Models;
using Avtokolonna1456.PasswordManager.Services;
using Avtokolonna1456.PasswordManager.Views;

namespace Avtokolonna1456.PasswordManager.ViewModels
{
    public sealed class MainViewModel : INotifyPropertyChanged
    {
        private readonly StorageService _storage;
        private readonly BackupService _backup;
        private readonly ClipboardService _clipboard;
        private readonly ThemeService _theme;
        private readonly AutostartService _autostart;
        private readonly NotificationService _notifications;
        private readonly DispatcherTimer _statusTimer;
        private string _searchText = string.Empty;
        private PasswordRowViewModel _selected;
        private bool _darkTheme;
        private bool _autostartEnabled;
        private string _statusMessage = string.Empty;

        public MainViewModel(
            AppData data,
            StorageService storage,
            BackupService backup,
            ClipboardService clipboard,
            ThemeService theme,
            AutostartService autostart,
            NotificationService notifications)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            if (Data.Entries == null)
            {
                Data.Entries = new System.Collections.Generic.List<PasswordEntry>();
            }

            _storage = storage;
            _backup = backup;
            _clipboard = clipboard;
            _theme = theme;
            _autostart = autostart;
            _notifications = notifications;

            _statusTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(4) };
            _statusTimer.Tick += (s, e) =>
            {
                _statusTimer.Stop();
                StatusMessage = string.Empty;
            };

            Rows = new ObservableCollection<PasswordRowViewModel>(
                Data.Entries.Select(e => new PasswordRowViewModel(e)));
            RowsView = CollectionViewSource.GetDefaultView(Rows);
            RowsView.Filter = FilterRow;
            RowsView.SortDescriptions.Add(new SortDescription(nameof(PasswordRowViewModel.DaysRemaining), ListSortDirection.Ascending));

            _darkTheme = data.DarkTheme;
            _autostartEnabled = data.AutostartEnabled;
            _theme.Apply(_darkTheme);
            _autostart.SetEnabled(_autostartEnabled);

            AddCommand = new RelayCommand(AddPassword);
            EditCommand = new RelayCommand(EditPassword, () => Selected != null);
            DeleteCommand = new RelayCommand(DeletePassword, () => Selected != null);
            TogglePasswordCommand = new RelayCommand(p => TogglePassword(p as PasswordRowViewModel));
            CopyLoginCommand = new RelayCommand(p => CopyLogin(p as PasswordRowViewModel));
            CopyPasswordCommand = new RelayCommand(p => CopyPassword(p as PasswordRowViewModel));
            BackupCommand = new RelayCommand(OpenBackups);
            ToggleThemeCommand = new RelayCommand(ToggleTheme);
            ToggleAutostartCommand = new RelayCommand(ToggleAutostart);
            CheckRemindersCommand = new RelayCommand(ShowReminders);
            ClearSearchCommand = new RelayCommand(() => SearchText = string.Empty);

            RefreshStats();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public AppData Data { get; }
        public ObservableCollection<PasswordRowViewModel> Rows { get; }
        public ICollectionView RowsView { get; }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand TogglePasswordCommand { get; }
        public ICommand CopyLoginCommand { get; }
        public ICommand CopyPasswordCommand { get; }
        public ICommand BackupCommand { get; }
        public ICommand ToggleThemeCommand { get; }
        public ICommand ToggleAutostartCommand { get; }
        public ICommand CheckRemindersCommand { get; }
        public ICommand ClearSearchCommand { get; }

        public PasswordRowViewModel Selected
        {
            get => _selected;
            set
            {
                if (ReferenceEquals(_selected, value)) return;
                _selected = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                var next = value ?? string.Empty;
                if (_searchText == next) return;
                _searchText = next;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSearchText));
                RowsView.Refresh();
                RefreshViewState();
            }
        }

        public bool HasSearchText => !string.IsNullOrWhiteSpace(_searchText);

        public bool DarkTheme
        {
            get => _darkTheme;
            set
            {
                if (_darkTheme == value) return;
                _darkTheme = value;
                Data.DarkTheme = value;
                _theme.Apply(value);
                Persist();
                OnPropertyChanged();
            }
        }

        public bool AutostartEnabled
        {
            get => _autostartEnabled;
            set
            {
                if (_autostartEnabled == value) return;
                _autostartEnabled = value;
                Data.AutostartEnabled = value;
                _autostart.SetEnabled(value);
                Persist();
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set
            {
                if (_statusMessage == value) return;
                _statusMessage = value ?? string.Empty;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasStatusMessage));
            }
        }

        public bool HasStatusMessage => !string.IsNullOrEmpty(_statusMessage);

        public int TotalCount { get; private set; }
        public int FilteredCount { get; private set; }
        public int OkCount { get; private set; }
        public int WarningCount { get; private set; }
        public int ExpiredCount { get; private set; }

        public bool ShowNoPasswordsState => TotalCount == 0;
        public bool ShowNoSearchResultsState => TotalCount > 0 && FilteredCount == 0;

        public string TableHeaderText
        {
            get
            {
                if (TotalCount == 0) return "Все пароли";
                if (HasSearchText) return $"Найдено {FilteredCount} из {TotalCount}";
                return $"Все пароли ({TotalCount})";
            }
        }

        public void Persist()
        {
            Data.Entries = Rows.Select(r => r.Entry).ToList();
            _storage.Save(Data);
            _backup.CreateBackup("auto");
        }

        public void RefreshAll()
        {
            foreach (var row in Rows) row.RefreshDerived();
            RowsView.Refresh();
            RefreshStats();
        }

        public void ShowStartupReminders()
        {
            var due = _notifications.GetDueReminders(Data.Entries);
            if (due.Count == 0) return;
            var window = new RemindersWindow(due) { Owner = Application.Current.MainWindow };
            window.ShowDialog();
        }

        public void CheckSystemNotifications()
        {
            var sent = _notifications.ProcessNotifications(Data);
            if (sent > 0) Persist();
        }

        private bool FilterRow(object obj)
        {
            var row = obj as PasswordRowViewModel;
            if (row == null) return false;
            if (string.IsNullOrWhiteSpace(SearchText)) return true;
            var q = SearchText.Trim();
            return (row.Title ?? string.Empty).IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                || (row.Login ?? string.Empty).IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void RefreshStats()
        {
            TotalCount = Rows.Count;
            OkCount = Rows.Count(r => r.Status == PasswordStatus.Ok);
            WarningCount = Rows.Count(r =>
                r.Status == PasswordStatus.Warning ||
                r.Status == PasswordStatus.Urgent ||
                r.Status == PasswordStatus.Today);
            ExpiredCount = Rows.Count(r => r.Status == PasswordStatus.Expired);
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(OkCount));
            OnPropertyChanged(nameof(WarningCount));
            OnPropertyChanged(nameof(ExpiredCount));
            RefreshViewState();
        }

        private void RefreshViewState()
        {
            FilteredCount = Rows.Count(FilterRow);
            OnPropertyChanged(nameof(FilteredCount));
            OnPropertyChanged(nameof(ShowNoPasswordsState));
            OnPropertyChanged(nameof(ShowNoSearchResultsState));
            OnPropertyChanged(nameof(TableHeaderText));
        }

        private void ShowStatus(string message)
        {
            StatusMessage = message;
            _statusTimer.Stop();
            _statusTimer.Start();
        }

        private void AddPassword()
        {
            var dialog = new PasswordEditWindow(null) { Owner = Application.Current.MainWindow };
            if (dialog.ShowDialog() != true) return;

            var entry = dialog.Result;
            var row = new PasswordRowViewModel(entry);
            Data.Entries.Add(entry);
            Rows.Add(row);
            RowsView.Refresh();
            Selected = row;
            Persist();
            RefreshStats();
            ShowStatus($"Пароль «{entry.Title}» добавлен.");
        }

        private void EditPassword()
        {
            if (Selected == null) return;
            var dialog = new PasswordEditWindow(Selected.Entry) { Owner = Application.Current.MainWindow };
            if (dialog.ShowDialog() != true) return;
            Selected.RefreshDerived();
            RowsView.Refresh();
            Persist();
            RefreshStats();
            ShowStatus($"Запись «{Selected.Title}» обновлена.");
        }

        private void DeletePassword()
        {
            if (Selected == null) return;
            var title = Selected.Title;
            var answer = MessageBox.Show(
                "Удалить запись «" + title + "»?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (answer != MessageBoxResult.Yes) return;

            Data.Entries.Remove(Selected.Entry);
            Rows.Remove(Selected);
            Selected = null;
            RowsView.Refresh();
            Persist();
            RefreshStats();
            ShowStatus($"Запись «{title}» удалена.");
        }

        private void TogglePassword(PasswordRowViewModel row)
        {
            if (row == null) return;
            row.IsPasswordVisible = !row.IsPasswordVisible;
        }

        private void CopyLogin(PasswordRowViewModel row)
        {
            if (row == null) return;
            _clipboard.CopyText(row.Login);
            ShowStatus("Логин скопирован в буфер обмена.");
        }

        private void CopyPassword(PasswordRowViewModel row)
        {
            if (row == null) return;
            _clipboard.CopySensitive(row.Password, 30);
            ShowStatus("Пароль скопирован (буфер очистится через 30 сек).");
        }

        private void OpenBackups()
        {
            var window = new BackupWindow(_backup, () =>
            {
                var reloaded = _storage.Load();
                Data.Entries = reloaded.Entries;
                Data.Pin = reloaded.Pin;
                Data.Version = reloaded.Version;
                Data.NotificationLog = reloaded.NotificationLog;
                Data.DarkTheme = reloaded.DarkTheme;
                Data.AutostartEnabled = reloaded.AutostartEnabled;
                Data.UpdatedAtUtc = reloaded.UpdatedAtUtc;
                Rows.Clear();
                foreach (var entry in Data.Entries)
                {
                    Rows.Add(new PasswordRowViewModel(entry));
                }
                DarkTheme = Data.DarkTheme;
                AutostartEnabled = Data.AutostartEnabled;
                RowsView.Refresh();
                RefreshStats();
                ShowStatus("Данные восстановлены из резервной копии.");
            })
            { Owner = Application.Current.MainWindow };
            window.ShowDialog();
        }

        private void ToggleTheme()
        {
            DarkTheme = !DarkTheme;
        }

        private void ToggleAutostart()
        {
            AutostartEnabled = !AutostartEnabled;
        }

        private void ShowReminders()
        {
            CheckSystemNotifications();
            ShowStartupReminders();
            var due = _notifications.GetDueReminders(Data.Entries);
            if (due.Count == 0)
            {
                MessageBox.Show("Сейчас нет паролей, требующих внимания.", "Напоминания",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

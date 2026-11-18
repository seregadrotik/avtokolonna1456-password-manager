using System;
using System.Linq;
using System.Windows;
using Avtokolonna1456.PasswordManager.Services;

namespace Avtokolonna1456.PasswordManager.Views
{
    public partial class BackupWindow : Window
    {
        private readonly BackupService _backup;
        private readonly Action _onRestored;

        public BackupWindow(BackupService backup, Action onRestored)
        {
            InitializeComponent();
            _backup = backup;
            _onRestored = onRestored;
            Reload();
        }

        private void Reload()
        {
            BackupList.ItemsSource = _backup.ListBackups()
                .Select(f => new BackupItem
                {
                    Path = f.FullName,
                    Display = f.Name + "  •  " + f.CreationTime.ToString("dd.MM.yyyy HH:mm")
                })
                .ToList();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            var path = _backup.CreateBackup("manual");
            if (path == null)
            {
                MessageBox.Show("Нет данных для копирования.", "Резервная копия",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Reload();
            MessageBox.Show("Копия создана:\n" + path, "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Restore_Click(object sender, RoutedEventArgs e)
        {
            var item = BackupList.SelectedItem as BackupItem;
            if (item == null)
            {
                MessageBox.Show("Выберите копию.", "Восстановление", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var answer = MessageBox.Show(
                "Текущие данные будут заменены выбранной копией. Продолжить?",
                "Восстановление",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (answer != MessageBoxResult.Yes) return;

            try
            {
                _backup.Restore(item.Path);
                _onRestored?.Invoke();
                MessageBox.Show("Данные восстановлены.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private sealed class BackupItem
        {
            public string Path { get; set; }
            public string Display { get; set; }
        }
    }
}

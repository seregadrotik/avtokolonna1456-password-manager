using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Avtokolonna1456.PasswordManager.Models;
using Avtokolonna1456.PasswordManager.Services;

namespace Avtokolonna1456.PasswordManager.Views
{
    public partial class RemindersWindow : Window
    {
        public RemindersWindow(IEnumerable<PasswordEntry> entries)
        {
            InitializeComponent();
            ReminderList.ItemsSource = entries.Select(e =>
            {
                var remaining = PasswordPolicyService.GetDaysRemaining(e.LastChangedAt);
                var status = PasswordPolicyService.GetStatus(e.LastChangedAt);
                return new ReminderItem
                {
                    Title = e.Title,
                    Detail = remaining < 0
                        ? "Просрочен на " + System.Math.Abs(remaining) + " дн. Следующая смена: " +
                          PasswordPolicyService.GetNextChangeDate(e.LastChangedAt).ToString("dd.MM.yyyy")
                        : "Осталось " + remaining + " дн. Сменить до " +
                          PasswordPolicyService.GetNextChangeDate(e.LastChangedAt).ToString("dd.MM.yyyy"),
                    StatusText = PasswordPolicyService.GetStatusText(status),
                    StatusBrush = ResolveBrush(status)
                };
            }).ToList();
        }

        private static Brush ResolveBrush(PasswordStatus status)
        {
            string key;
            switch (status)
            {
                case PasswordStatus.Warning: key = "Brush.StatusWarning"; break;
                case PasswordStatus.Urgent: key = "Brush.StatusUrgent"; break;
                case PasswordStatus.Today: key = "Brush.StatusToday"; break;
                case PasswordStatus.Expired: key = "Brush.StatusExpired"; break;
                default: key = "Brush.StatusOk"; break;
            }
            return Application.Current.TryFindResource(key) as Brush ?? Brushes.Gray;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private sealed class ReminderItem
        {
            public string Title { get; set; }
            public string Detail { get; set; }
            public string StatusText { get; set; }
            public Brush StatusBrush { get; set; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Windows;

namespace Avtokolonna1456.PasswordManager.Views
{
    public partial class RecoveryCodeWindow : Window
    {
        public RecoveryCodeWindow(string code)
        {
            InitializeComponent();
            CodeText.Text = FormatCode(code);
        }

        private static string FormatCode(string code)
        {
            if (string.IsNullOrEmpty(code)) return string.Empty;
            code = code.Replace(" ", string.Empty);
            if (code.Length <= 4) return code;
            var parts = new List<string>();
            for (var i = 0; i < code.Length; i += 4)
            {
                parts.Add(code.Substring(i, Math.Min(4, code.Length - i)));
            }
            return string.Join(" ", parts);
        }

        private bool _confirmed;

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            if (ConfirmCheck.IsChecked != true)
            {
                MessageBox.Show("Подтвердите, что сохранили резервный код.", "Важно",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _confirmed = true;
            DialogResult = true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_confirmed) return;
            var answer = MessageBox.Show(
                "Без сохранения резервного кода нельзя продолжить. Закрыть окно?",
                "Резервный код",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (answer != MessageBoxResult.Yes)
            {
                e.Cancel = true;
            }
        }
    }
}

using System;
using System.Windows;
using Avtokolonna1456.PasswordManager.Models;

namespace Avtokolonna1456.PasswordManager.Views
{
    public partial class PasswordEditWindow : Window
    {
        private readonly PasswordEntry _original;
        private readonly bool _isEdit;
        private bool _passwordVisible;

        public PasswordEditWindow(PasswordEntry existing)
        {
            InitializeComponent();
            _isEdit = existing != null;
            _original = existing;

            if (_isEdit)
            {
                Title = "Редактировать пароль";
                HeaderText.Text = "Редактировать пароль";
                TitleBox.Text = existing.Title;
                LoginBox.Text = existing.Login;
                PasswordHidden.Password = existing.Password;
                PasswordVisible.Text = existing.Password;
                ChangedDatePicker.SelectedDate = existing.LastChangedAt.Date;
            }
            else
            {
                Title = "Добавить пароль";
                HeaderText.Text = "Добавить пароль";
                ChangedDatePicker.SelectedDate = DateTime.Today;
            }

            Loaded += (s, e) => TitleBox.Focus();
        }

        public PasswordEntry Result { get; private set; }

        private void TogglePassword_Click(object sender, RoutedEventArgs e)
        {
            _passwordVisible = !_passwordVisible;
            if (_passwordVisible)
            {
                PasswordVisible.Text = PasswordHidden.Password;
                PasswordVisible.Visibility = Visibility.Visible;
                PasswordHidden.Visibility = Visibility.Collapsed;
                TogglePasswordButton.Content = "Скрыть";
                PasswordVisible.Focus();
                PasswordVisible.CaretIndex = PasswordVisible.Text.Length;
            }
            else
            {
                PasswordHidden.Password = PasswordVisible.Text;
                PasswordHidden.Visibility = Visibility.Visible;
                PasswordVisible.Visibility = Visibility.Collapsed;
                TogglePasswordButton.Content = "Показать";
                PasswordHidden.Focus();
            }
        }

        private void ShowError(string message, UIElement focusTarget)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
            focusTarget?.Focus();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Visibility = Visibility.Collapsed;

            var title = (TitleBox.Text ?? string.Empty).Trim();
            var login = (LoginBox.Text ?? string.Empty).Trim();
            var password = _passwordVisible ? PasswordVisible.Text : PasswordHidden.Password;
            var changed = (ChangedDatePicker.SelectedDate ?? DateTime.Today).Date;

            if (string.IsNullOrWhiteSpace(title))
            {
                ShowError("Укажите название сервиса или ресурса.", TitleBox);
                return;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError("Укажите пароль.", _passwordVisible ? (UIElement)PasswordVisible : PasswordHidden);
                return;
            }

            if (_isEdit)
            {
                var passwordChanged = !string.Equals(_original.Password, password, StringComparison.Ordinal);
                var dateChangedByUser = _original.LastChangedAt.Date != changed;

                _original.Title = title;
                _original.Login = login;

                if (passwordChanged)
                {
                    _original.Password = password;
                    _original.LastChangedAt = dateChangedByUser ? changed : DateTime.Today;
                }
                else if (dateChangedByUser)
                {
                    _original.LastChangedAt = changed;
                }

                Result = _original;
            }
            else
            {
                Result = new PasswordEntry
                {
                    Title = title,
                    Login = login,
                    Password = password,
                    CreatedAt = DateTime.Today,
                    LastChangedAt = changed
                };
            }

            DialogResult = true;
            Close();
        }
    }
}

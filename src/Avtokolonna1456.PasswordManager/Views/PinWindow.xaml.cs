using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Avtokolonna1456.PasswordManager.Models;
using Avtokolonna1456.PasswordManager.Services;

namespace Avtokolonna1456.PasswordManager.Views
{
    public partial class PinWindow : Window
    {
        private enum Mode
        {
            Create,
            Confirm,
            Unlock
        }

        private readonly AppData _data;
        private readonly StorageService _storage;
        private readonly PinService _pinService;
        private readonly StringBuilder _buffer = new StringBuilder();
        private Mode _mode;
        private string _pendingPin;
        private string _recoveryCode;

        public PinWindow(AppData data, StorageService storage, PinService pinService)
        {
            InitializeComponent();
            _data = data;
            _storage = storage;
            _pinService = pinService;

            if (data.Pin != null && data.Pin.IsConfigured)
            {
                _mode = Mode.Unlock;
                TitleText.Text = "Введите PIN";
                RecoveryButton.Visibility = Visibility.Visible;
            }
            else
            {
                _mode = Mode.Create;
                TitleText.Text = "Создайте PIN из 4 цифр";
                RecoveryButton.Visibility = Visibility.Collapsed;
            }

            UpdateDots();
        }

        private void Digit_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as FrameworkElement;
            if (button?.Tag == null) return;
            AppendDigit(button.Tag.ToString());
        }

        private void Backspace_Click(object sender, RoutedEventArgs e)
        {
            if (_buffer.Length == 0) return;
            _buffer.Length--;
            UpdateDots();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Submit();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key >= Key.D0 && e.Key <= Key.D9)
            {
                AppendDigit(((int)(e.Key - Key.D0)).ToString());
                e.Handled = true;
            }
            else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            {
                AppendDigit(((int)(e.Key - Key.NumPad0)).ToString());
                e.Handled = true;
            }
            else if (e.Key == Key.Back)
            {
                Backspace_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                Submit();
                e.Handled = true;
            }
        }

        private void AppendDigit(string digit)
        {
            if (_buffer.Length >= 4) return;
            _buffer.Append(digit);
            UpdateDots();
            if (_buffer.Length == 4)
            {
                Submit();
            }
        }

        private void Submit()
        {
            if (_buffer.Length != 4)
            {
                StatusText.Text = "Введите 4 цифры.";
                return;
            }

            var pin = _buffer.ToString();
            _buffer.Clear();
            UpdateDots();

            switch (_mode)
            {
                case Mode.Create:
                    _pendingPin = pin;
                    _mode = Mode.Confirm;
                    TitleText.Text = "Подтвердите PIN";
                    StatusText.Text = string.Empty;
                    break;

                case Mode.Confirm:
                    if (!string.Equals(_pendingPin, pin, StringComparison.Ordinal))
                    {
                        StatusText.Text = "PIN не совпал. Создайте снова.";
                        _mode = Mode.Create;
                        _pendingPin = null;
                        TitleText.Text = "Создайте PIN из 4 цифр";
                        return;
                    }

                    _recoveryCode = _pinService.CreateRecoveryCode();
                    var recoveryWindow = new RecoveryCodeWindow(_recoveryCode) { Owner = this };
                    if (recoveryWindow.ShowDialog() != true)
                    {
                        StatusText.Text = "Нужно сохранить резервный код, чтобы продолжить.";
                        _mode = Mode.Create;
                        _pendingPin = null;
                        TitleText.Text = "Создайте PIN из 4 цифр";
                        return;
                    }

                    _pinService.ConfigurePin(_data.Pin, pin, _recoveryCode);
                    _storage.Save(_data);
                    DialogResult = true;
                    Close();
                    break;

                case Mode.Unlock:
                    if (_pinService.VerifyPin(_data.Pin, pin, out var error))
                    {
                        _storage.Save(_data);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        StatusText.Text = error;
                        _storage.Save(_data);
                    }
                    break;
            }
        }

        private void Recovery_Click(object sender, RoutedEventArgs e)
        {
            RecoveryPanel.Visibility = RecoveryPanel.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void ResetPin_Click(object sender, RoutedEventArgs e)
        {
            var code = RecoveryCodeBox.Text;
            var newPin = NewPinBox.Password;
            if (!_pinService.VerifyRecoveryAndResetPin(_data.Pin, code, newPin, out var error))
            {
                StatusText.Text = error;
                _storage.Save(_data);
                return;
            }

            _storage.Save(_data);
            MessageBox.Show("PIN успешно обновлён.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }

        private void UpdateDots()
        {
            SetDot(Dot0, _buffer.Length > 0);
            SetDot(Dot1, _buffer.Length > 1);
            SetDot(Dot2, _buffer.Length > 2);
            SetDot(Dot3, _buffer.Length > 3);
        }

        private void SetDot(Ellipse ellipse, bool filled)
        {
            ellipse.Fill = filled
                ? (Brush)FindResource("Brush.Accent")
                : (Brush)FindResource("Brush.Border");
        }
    }
}

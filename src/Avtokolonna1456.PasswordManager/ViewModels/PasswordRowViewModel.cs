using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avtokolonna1456.PasswordManager.Models;
using Avtokolonna1456.PasswordManager.Services;

namespace Avtokolonna1456.PasswordManager.ViewModels
{
    public sealed class PasswordRowViewModel : INotifyPropertyChanged
    {
        private bool _isPasswordVisible;

        public PasswordRowViewModel(PasswordEntry entry)
        {
            Entry = entry ?? throw new ArgumentNullException(nameof(entry));
            RefreshDerived();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public PasswordEntry Entry { get; }

        public Guid Id => Entry.Id;
        public string Title => Entry.Title;
        public string Login => Entry.Login;
        public string Password => Entry.Password;
        public DateTime LastChangedAt => Entry.LastChangedAt.Date;
        public DateTime NextChangeAt { get; private set; }
        public int AgeDays { get; private set; }
        public int DaysRemaining { get; private set; }
        public PasswordStatus Status { get; private set; }
        public string StatusText { get; private set; }
        public string DaysRemainingText { get; private set; }

        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set
            {
                if (_isPasswordVisible == value) return;
                _isPasswordVisible = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayPassword));
            }
        }

        public string DisplayPassword => IsPasswordVisible ? Password : new string('•', Math.Max(8, Password?.Length ?? 8));

        public void RefreshDerived()
        {
            AgeDays = PasswordPolicyService.GetAgeDays(Entry.LastChangedAt);
            DaysRemaining = PasswordPolicyService.GetDaysRemaining(Entry.LastChangedAt);
            NextChangeAt = PasswordPolicyService.GetNextChangeDate(Entry.LastChangedAt);
            Status = PasswordPolicyService.GetStatus(Entry.LastChangedAt);
            StatusText = PasswordPolicyService.GetStatusText(Status);
            DaysRemainingText = DaysRemaining < 0
                ? "Просрочен " + Math.Abs(DaysRemaining) + " дн."
                : DaysRemaining + " дн.";

            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Login));
            OnPropertyChanged(nameof(Password));
            OnPropertyChanged(nameof(DisplayPassword));
            OnPropertyChanged(nameof(LastChangedAt));
            OnPropertyChanged(nameof(NextChangeAt));
            OnPropertyChanged(nameof(AgeDays));
            OnPropertyChanged(nameof(DaysRemaining));
            OnPropertyChanged(nameof(DaysRemainingText));
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(StatusText));
        }

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

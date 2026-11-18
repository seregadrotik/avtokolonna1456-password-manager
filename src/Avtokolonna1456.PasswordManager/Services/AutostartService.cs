using System;
using Microsoft.Win32;

namespace Avtokolonna1456.PasswordManager.Services
{
    public sealed class AutostartService
    {
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "Avtokolonna1456PasswordManager";

        public bool IsEnabled()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RunKey, false))
            {
                var value = key?.GetValue(ValueName) as string;
                return !string.IsNullOrWhiteSpace(value);
            }
        }

        public void SetEnabled(bool enabled)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RunKey, true) ?? Registry.CurrentUser.CreateSubKey(RunKey))
            {
                if (key == null) return;
                if (enabled)
                {
                    var exe = System.Windows.Forms.Application.ExecutablePath;
                    key.SetValue(ValueName, "\"" + exe + "\" --minimized");
                }
                else if (key.GetValue(ValueName) != null)
                {
                    key.DeleteValue(ValueName, false);
                }
            }
        }
    }
}

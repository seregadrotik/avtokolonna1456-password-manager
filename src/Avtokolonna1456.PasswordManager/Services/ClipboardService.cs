using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Avtokolonna1456.PasswordManager.Services
{
    public sealed class ClipboardService
    {
        private CancellationTokenSource _cts;

        public void CopySensitive(string value, int clearAfterSeconds = 30)
        {
            if (value == null) value = string.Empty;
            Clipboard.SetText(value);

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            var snapshot = value;

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(clearAfterSeconds), token);
                    if (token.IsCancellationRequested) return;

                    Application.Current?.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            if (Clipboard.ContainsText())
                            {
                                var current = Clipboard.GetText();
                                if (string.Equals(current, snapshot, StringComparison.Ordinal))
                                {
                                    Clipboard.Clear();
                                }
                            }
                        }
                        catch
                        {
                            // Clipboard may be busy; ignore.
                        }
                    });
                }
                catch (TaskCanceledException)
                {
                }
            }, token);
        }

        public void CopyText(string value)
        {
            Clipboard.SetText(value ?? string.Empty);
        }
    }
}

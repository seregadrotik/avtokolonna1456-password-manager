using System;
using System.Collections.Generic;
using System.Linq;
using Avtokolonna1456.PasswordManager.Models;
using Microsoft.Toolkit.Uwp.Notifications;

namespace Avtokolonna1456.PasswordManager.Services
{
    public sealed class NotificationService
    {
        private readonly Action<string> _balloonFallback;

        public NotificationService(Action<string> balloonFallback = null)
        {
            _balloonFallback = balloonFallback;
        }

        public IReadOnlyList<PasswordEntry> GetDueReminders(IEnumerable<PasswordEntry> entries)
        {
            return entries
                .Where(e =>
                {
                    var remaining = PasswordPolicyService.GetDaysRemaining(e.LastChangedAt);
                    return remaining <= 7;
                })
                .OrderBy(e => PasswordPolicyService.GetDaysRemaining(e.LastChangedAt))
                .ThenBy(e => e.Title)
                .ToList();
        }

        public int ProcessNotifications(AppData data, bool forceUiCheck = false)
        {
            if (data?.Entries == null) return 0;
            var today = DateTime.Today.ToString("yyyy-MM-dd");
            var sent = 0;

            foreach (var entry in data.Entries)
            {
                var remaining = PasswordPolicyService.GetDaysRemaining(entry.LastChangedAt);
                if (!ShouldNotify(remaining)) continue;

                var key = entry.Id + "|" + GetBucket(remaining) + "|" + today;
                if (data.NotificationLog != null && data.NotificationLog.ContainsKey(key))
                {
                    continue;
                }

                var message = BuildMessage(entry.Title, remaining);
                ShowToast("Автоколонна 1456", message);
                if (data.NotificationLog == null)
                {
                    data.NotificationLog = new Dictionary<string, string>();
                }
                data.NotificationLog[key] = DateTime.UtcNow.ToString("o");
                sent++;
            }

            CleanupOldLog(data, today);
            return sent;
        }

        public void ShowToast(string title, string body)
        {
            try
            {
                new ToastContentBuilder()
                    .AddText(title)
                    .AddText(body)
                    .Show();
            }
            catch
            {
                _balloonFallback?.Invoke(title + Environment.NewLine + body);
            }
        }

        private static bool ShouldNotify(int remaining)
        {
            if (remaining < 0) return true;
            return remaining == 0 || remaining == 1 || remaining == 3 || remaining == 7;
        }

        private static string GetBucket(int remaining)
        {
            if (remaining < 0) return "overdue";
            if (remaining == 0) return "d0";
            if (remaining == 1) return "d1";
            if (remaining == 3) return "d3";
            if (remaining == 7) return "d7";
            return "other";
        }

        private static string BuildMessage(string title, int remaining)
        {
            var name = string.IsNullOrWhiteSpace(title) ? "Без названия" : title;
            if (remaining < 0)
            {
                var overdue = Math.Abs(remaining);
                return $"«{name}»: просрочен на {overdue} дн. Обновите пароль.";
            }
            if (remaining == 0)
            {
                return $"«{name}»: срок обновления истёк сегодня.";
            }
            return $"«{name}»: осталось {remaining} дн. до обновления.";
        }

        private static void CleanupOldLog(AppData data, string today)
        {
            if (data.NotificationLog == null) return;
            var keep = data.NotificationLog
                .Where(kv => kv.Key.EndsWith("|" + today, StringComparison.Ordinal))
                .ToDictionary(kv => kv.Key, kv => kv.Value);
            data.NotificationLog = keep;
        }
    }
}

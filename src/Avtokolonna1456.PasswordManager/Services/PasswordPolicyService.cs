using System;
using Avtokolonna1456.PasswordManager.Models;

namespace Avtokolonna1456.PasswordManager.Services
{
    public static class PasswordPolicyService
    {
        public const int RotationDays = 90;

        public static int GetAgeDays(DateTime lastChangedAt, DateTime? today = null)
        {
            var day = (today ?? DateTime.Today).Date;
            var changed = lastChangedAt.Date;
            return Math.Max(0, (day - changed).Days);
        }

        public static int GetDaysRemaining(DateTime lastChangedAt, DateTime? today = null)
        {
            return RotationDays - GetAgeDays(lastChangedAt, today);
        }

        public static DateTime GetNextChangeDate(DateTime lastChangedAt)
        {
            return lastChangedAt.Date.AddDays(RotationDays);
        }

        public static PasswordStatus GetStatus(DateTime lastChangedAt, DateTime? today = null)
        {
            var remaining = GetDaysRemaining(lastChangedAt, today);
            if (remaining < 0) return PasswordStatus.Expired;
            if (remaining == 0) return PasswordStatus.Today;
            if (remaining <= 3) return PasswordStatus.Urgent;
            if (remaining <= 7) return PasswordStatus.Warning;
            return PasswordStatus.Ok;
        }

        public static string GetStatusText(PasswordStatus status)
        {
            switch (status)
            {
                case PasswordStatus.Ok: return "В порядке";
                case PasswordStatus.Warning: return "Скоро обновить";
                case PasswordStatus.Urgent: return "Срочно";
                case PasswordStatus.Today: return "Сегодня";
                case PasswordStatus.Expired: return "Просрочен";
                default: return status.ToString();
            }
        }
    }
}

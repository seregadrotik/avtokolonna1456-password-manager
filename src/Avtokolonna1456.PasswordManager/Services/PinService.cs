using System;
using System.Security.Cryptography;
using System.Text;
using Avtokolonna1456.PasswordManager.Models;

namespace Avtokolonna1456.PasswordManager.Services
{
    public sealed class PinService
    {
        public const int MaxAttemptsBeforeLock = 5;
        public static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(1);

        public bool IsLocked(PinState pin, out TimeSpan remaining)
        {
            remaining = TimeSpan.Zero;
            if (pin?.LockUntilUtc == null) return false;
            var left = pin.LockUntilUtc.Value - DateTime.UtcNow;
            if (left <= TimeSpan.Zero)
            {
                pin.LockUntilUtc = null;
                return false;
            }

            remaining = left;
            return true;
        }

        public string CreateRecoveryCode()
        {
            var bytes = new byte[5];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            var value = BitConverterExtensions.ToUInt40Safe(bytes);
            return value.ToString("D12");
        }

        public void ConfigurePin(PinState pin, string pinCode, string recoveryCode)
        {
            ValidatePin(pinCode);
            if (string.IsNullOrWhiteSpace(recoveryCode) || recoveryCode.Length < 8)
            {
                throw new ArgumentException("Резервный код слишком короткий.");
            }

            var pinSalt = CreateSalt();
            var recoverySalt = CreateSalt();
            pin.PinSalt = Convert.ToBase64String(pinSalt);
            pin.PinHash = Hash(pinCode, pinSalt);
            pin.RecoverySalt = Convert.ToBase64String(recoverySalt);
            pin.RecoveryHash = Hash(NormalizeRecovery(recoveryCode), recoverySalt);
            pin.FailedAttempts = 0;
            pin.LockUntilUtc = null;
            pin.IsConfigured = true;
        }

        public bool VerifyPin(PinState pin, string pinCode, out string error)
        {
            error = null;
            if (IsLocked(pin, out var remaining))
            {
                error = $"Слишком много ошибок. Повторите через {Math.Ceiling(remaining.TotalSeconds)} сек.";
                return false;
            }

            if (!pin.IsConfigured)
            {
                error = "PIN ещё не настроен.";
                return false;
            }

            if (!IsValidPinFormat(pinCode) || !SlowEquals(pin.PinHash, Hash(pinCode, Convert.FromBase64String(pin.PinSalt))))
            {
                pin.FailedAttempts++;
                if (pin.FailedAttempts >= MaxAttemptsBeforeLock)
                {
                    pin.LockUntilUtc = DateTime.UtcNow.Add(LockDuration);
                    pin.FailedAttempts = 0;
                    error = $"Слишком много ошибок. Блокировка на {(int)LockDuration.TotalMinutes} мин.";
                }
                else
                {
                    var left = MaxAttemptsBeforeLock - pin.FailedAttempts;
                    error = $"Неверный PIN. Осталось попыток: {left}.";
                }
                return false;
            }

            pin.FailedAttempts = 0;
            pin.LockUntilUtc = null;
            return true;
        }

        public bool VerifyRecoveryAndResetPin(PinState pin, string recoveryCode, string newPin, out string error)
        {
            error = null;
            if (IsLocked(pin, out var remaining))
            {
                error = $"Слишком много ошибок. Повторите через {Math.Ceiling(remaining.TotalSeconds)} сек.";
                return false;
            }

            var ok = SlowEquals(
                pin.RecoveryHash,
                Hash(NormalizeRecovery(recoveryCode), Convert.FromBase64String(pin.RecoverySalt)));

            if (!ok)
            {
                pin.FailedAttempts++;
                if (pin.FailedAttempts >= MaxAttemptsBeforeLock)
                {
                    pin.LockUntilUtc = DateTime.UtcNow.Add(LockDuration);
                    pin.FailedAttempts = 0;
                    error = $"Неверный резервный код. Блокировка на {(int)LockDuration.TotalMinutes} мин.";
                }
                else
                {
                    error = "Неверный резервный код.";
                }
                return false;
            }

            ConfigurePin(pin, newPin, recoveryCode);
            return true;
        }

        public static bool IsValidPinFormat(string pin)
        {
            if (string.IsNullOrEmpty(pin) || pin.Length != 4) return false;
            foreach (var c in pin)
            {
                if (c < '0' || c > '9') return false;
            }
            return true;
        }

        private static void ValidatePin(string pin)
        {
            if (!IsValidPinFormat(pin))
            {
                throw new ArgumentException("PIN должен состоять из 4 цифр.");
            }
        }

        private static string NormalizeRecovery(string code)
        {
            return (code ?? string.Empty).Trim().Replace(" ", string.Empty).ToUpperInvariant();
        }

        private static byte[] CreateSalt()
        {
            var salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        private static string Hash(string value, byte[] salt)
        {
            using (var derive = new Rfc2898DeriveBytes(value, salt, 100000, HashAlgorithmName.SHA256))
            {
                return Convert.ToBase64String(derive.GetBytes(32));
            }
        }

        private static bool SlowEquals(string a, string b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            var result = 0;
            for (var i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }
            return result == 0;
        }
    }

    internal static class BitConverterExtensions
    {
        public static ulong ToUInt40Safe(byte[] bytes)
        {
            ulong value = 0;
            for (var i = 0; i < Math.Min(5, bytes.Length); i++)
            {
                value = (value << 8) | bytes[i];
            }
            return value % 1000000000000UL;
        }
    }
}

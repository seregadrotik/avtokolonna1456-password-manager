using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using Avtokolonna1456.PasswordManager.Models;

namespace Avtokolonna1456.PasswordManager.Services
{
    public sealed class StorageService
    {
        public AppData Load()
        {
            if (!File.Exists(AppPaths.DataFile))
            {
                return new AppData();
            }

            try
            {
                var encrypted = File.ReadAllBytes(AppPaths.DataFile);
                var jsonBytes = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                return Deserialize(jsonBytes) ?? new AppData();
            }
            catch (CryptographicException)
            {
                throw new InvalidOperationException(
                    "Не удалось расшифровать файл данных. Восстановление возможно только под той же учётной записью Windows.");
            }
        }

        public void Save(AppData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            data.UpdatedAtUtc = DateTime.UtcNow;
            var jsonBytes = Serialize(data);
            var encrypted = ProtectedData.Protect(jsonBytes, null, DataProtectionScope.CurrentUser);

            var temp = AppPaths.DataFile + ".tmp";
            File.WriteAllBytes(temp, encrypted);
            if (File.Exists(AppPaths.DataFile))
            {
                File.Replace(temp, AppPaths.DataFile, null);
            }
            else
            {
                File.Move(temp, AppPaths.DataFile);
            }
        }

        public void SaveRawEncrypted(byte[] encryptedBytes)
        {
            File.WriteAllBytes(AppPaths.DataFile, encryptedBytes);
        }

        public byte[] ReadEncryptedBytes()
        {
            return File.Exists(AppPaths.DataFile)
                ? File.ReadAllBytes(AppPaths.DataFile)
                : Array.Empty<byte>();
        }

        private static byte[] Serialize(AppData data)
        {
            var serializer = new DataContractJsonSerializer(typeof(AppData));
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, data);
                return stream.ToArray();
            }
        }

        private static AppData Deserialize(byte[] bytes)
        {
            var serializer = new DataContractJsonSerializer(typeof(AppData));
            using (var stream = new MemoryStream(bytes))
            {
                var data = serializer.ReadObject(stream) as AppData ?? new AppData();
                if (data.Entries == null) data.Entries = new System.Collections.Generic.List<PasswordEntry>();
                if (data.Pin == null) data.Pin = new PinState();
                if (data.NotificationLog == null) data.NotificationLog = new System.Collections.Generic.Dictionary<string, string>();
                return data;
            }
        }
    }
}

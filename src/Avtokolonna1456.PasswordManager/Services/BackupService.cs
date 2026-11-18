using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Avtokolonna1456.PasswordManager.Services
{
    public sealed class BackupService
    {
        private readonly StorageService _storage;
        private const int MaxBackups = 10;

        public BackupService(StorageService storage)
        {
            _storage = storage;
        }

        public string CreateBackup(string reason = "auto")
        {
            var source = _storage.ReadEncryptedBytes();
            if (source.Length == 0) return null;

            var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var safeReason = string.IsNullOrWhiteSpace(reason) ? "manual" : reason;
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                safeReason = safeReason.Replace(c, '_');
            }

            var file = Path.Combine(AppPaths.BackupFolder, $"vault_{stamp}_{safeReason}.bak");
            File.WriteAllBytes(file, source);
            TrimOldBackups();
            return file;
        }

        public IReadOnlyList<FileInfo> ListBackups()
        {
            return new DirectoryInfo(AppPaths.BackupFolder)
                .GetFiles("*.bak")
                .OrderByDescending(f => f.CreationTimeUtc)
                .ToList();
        }

        public void Restore(string backupPath)
        {
            if (!File.Exists(backupPath))
            {
                throw new FileNotFoundException("Резервная копия не найдена.", backupPath);
            }

            CreateBackup("before-restore");
            var bytes = File.ReadAllBytes(backupPath);
            _storage.SaveRawEncrypted(bytes);
        }

        private void TrimOldBackups()
        {
            var files = ListBackups();
            foreach (var old in files.Skip(MaxBackups))
            {
                try { old.Delete(); } catch { /* ignore */ }
            }
        }
    }
}

using System;
using System.IO;

namespace Avtokolonna1456.PasswordManager.Services
{
    public static class AppPaths
    {
        public static string Root
        {
            get
            {
                var path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Avtokolonna1456",
                    "PasswordManager");
                Directory.CreateDirectory(path);
                return path;
            }
        }

        public static string DataFile => Path.Combine(Root, "vault.dat");
        public static string BackupFolder
        {
            get
            {
                var path = Path.Combine(Root, "Backups");
                Directory.CreateDirectory(path);
                return path;
            }
        }
    }
}

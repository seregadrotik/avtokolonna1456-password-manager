using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Avtokolonna1456.PasswordManager.Models
{
    [DataContract]
    public sealed class AppData
    {
        [DataMember]
        public int Version { get; set; } = 1;

        [DataMember]
        public PinState Pin { get; set; } = new PinState();

        [DataMember]
        public List<PasswordEntry> Entries { get; set; } = new List<PasswordEntry>();

        [DataMember]
        public bool DarkTheme { get; set; }

        [DataMember]
        public bool AutostartEnabled { get; set; } = true;

        [DataMember]
        public Dictionary<string, string> NotificationLog { get; set; } = new Dictionary<string, string>();

        [DataMember]
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}

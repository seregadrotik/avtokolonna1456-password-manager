using System;
using System.Runtime.Serialization;

namespace Avtokolonna1456.PasswordManager.Models
{
    [DataContract]
    public sealed class PasswordEntry
    {
        [DataMember]
        public Guid Id { get; set; } = Guid.NewGuid();

        [DataMember]
        public string Title { get; set; } = string.Empty;

        [DataMember]
        public string Login { get; set; } = string.Empty;

        [DataMember]
        public string Password { get; set; } = string.Empty;

        [DataMember]
        public DateTime CreatedAt { get; set; } = DateTime.Today;

        [DataMember]
        public DateTime LastChangedAt { get; set; } = DateTime.Today;
    }
}

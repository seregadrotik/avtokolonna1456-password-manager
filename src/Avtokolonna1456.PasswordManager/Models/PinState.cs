using System;
using System.Runtime.Serialization;

namespace Avtokolonna1456.PasswordManager.Models
{
    [DataContract]
    public sealed class PinState
    {
        [DataMember]
        public string PinHash { get; set; } = string.Empty;

        [DataMember]
        public string PinSalt { get; set; } = string.Empty;

        [DataMember]
        public string RecoveryHash { get; set; } = string.Empty;

        [DataMember]
        public string RecoverySalt { get; set; } = string.Empty;

        [DataMember]
        public int FailedAttempts { get; set; }

        [DataMember]
        public DateTime? LockUntilUtc { get; set; }

        [DataMember]
        public bool IsConfigured { get; set; }
    }
}

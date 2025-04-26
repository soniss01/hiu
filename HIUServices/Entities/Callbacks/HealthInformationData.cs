using Microsoft.EntityFrameworkCore;

namespace HIUServices.Entities.Callbacks
{
    [Keyless]
    public class HIData
    {
        public DateTime? expiry { get; set; }
        public string transactionId { get; set; }
        public string publicKey { get; set; }
        public string nonce { get; set; }
        public string content { get; set; }
        public string care_context { get; set; }
    }

    [Keyless]
    public class HealthInformationData
    {
        public int pageNumber { get; set; }
        public int pageCount { get; set; }
        public string transactionId { get; set; }
        public List<Entry> entries { get; set; }
        public KeyMaterial keyMaterial { get; set; }

    }
    [Keyless]
    public class Entry
    {
        public string content { get; set; }
        public string media { get; set; }
        public string checksum { get; set; }
        public string careContextReference { get; set; }
    }
    [Keyless]
    public class KeyMaterial
    {
        public string cryptoAlg { get; set; }
        public string curve { get; set; }
        public DhPublicKey dhPublicKey { get; set; }
        public string nonce { get; set; }
    }
    [Keyless]
    public class DhPublicKey
    {
        public DateTime? expiry { get; set; }
        public string parameters { get; set; }
        public string keyValue { get; set; }
    }
}

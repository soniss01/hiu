namespace HIUServices.Entities.Requests.Data
{
    public class HealthInformationCmRequest
    {
        public string requestId { get; set; }
        public string timestamp { get; set; }
        public HiRequest hiRequest { get; set; }
    }

    public class KeyMaterial
    {
        public string cryptoAlg { get; set; }
        public string curve { get; set; }
        public DhPublicKey dhPublicKey { get; set; }
        public string? nonce { get; set; }
    }

    public class DhPublicKey
    {
        public string expiry { get; set; }
        public string parameters { get; set; }
        public string keyValue { get; set; }
    }

    public class DateRange
    {
        public string from { get; set; }
        public string to { get; set; }
    }

    public class HiRequest
    {
        public HiRequestConsent consent { get; set; }
        public DateRange dateRange { get; set; }
        public string dataPushUrl { get; set; }
        public KeyMaterial keyMaterial { get; set; }
    }

    public class HiRequestConsent
    {
        public string id { get; set; }
    }

}

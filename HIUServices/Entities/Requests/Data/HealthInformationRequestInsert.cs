namespace HIUServices.Entities.Requests.Data
{
    public class HealthInformationRequestInsert
    {
        public string request_id { get; set; }
        public string timestamp { get; set; }
        public string consent_id { get; set; }
        public DateTime? expiry_date { get; set; }
        public DateTime? from_date { get; set; }
        public DateTime? to_date { get; set; }
        public string receiver_public_key { get; set; }
        public string receiver_private_key { get; set; }
        public string receiver_nonce { get; set; }
        public int source_id { get; set; }
    }
}

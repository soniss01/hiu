namespace HIUServices.Entities.Requests.Consents
{
    public class ParentConsent
    {
        public string patient_id { get; set; }
        public string patient_name { get; set; }
        public string abha_number { get; set; }
        public DateTime? dob { get; set; }
        public string consent_request_id { get; set; }
        public DateTime? consent_requested_on { get; set; }
        public DateTime? requested_date_from { get; set; }
        public DateTime? requested_date_to { get; set; }
        public DateTime? requested_expiry_date { get; set; }
        public List<ChildConsent>? consents { get; set; }
        public string requested_hi_types { get; set; }
        public string status { get; set; }
    }

    public class ChildConsent
    {
        public string status { get; set; }
        public string reason { get; set; }
        public string consent_artefacts_id { get; set; }
        public string hip_id { get; set; }
        public string hip_name { get; set; }
        public string access_mode { get; set; }
        public DateTime? date_range_from { get; set; }
        public DateTime? date_range_to { get; set; }
        public DateTime? data_erase_at { get; set; }
        public DateTime? consent_responded_on { get; set; }
        public string granted_hi_types { get; set; }
    }


}

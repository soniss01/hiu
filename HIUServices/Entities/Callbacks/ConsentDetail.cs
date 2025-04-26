namespace HIUServices.Entities.Callbacks
{
    public class ConsentDetail
    {
        public string consentId { get; set; }
        public Hip? hip { get; set; }
        public Permission? permission { get; set; }
        public List<CareContext>? careContexts { get; set; }
        public List<string>? hiTypes { get; set; }
    }

    public class Hip
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Permission
    {
        public string accessMode { get; set; }
        public DateRange? dateRange { get; set; }
        public DateTime? dataEraseAt { get; set; }
    }

    public class DateRange
    {
        public DateTime? from { get; set; }
        public DateTime? to { get; set; }
    }

    public class CareContext
    {
        public string patientReference { get; set; }
        public string careContextReference { get; set; }
    }

    public class Consent
    {
        public string status { get; set; }
        public ConsentDetail? consentDetail { get; set; }
    }

    public class ConsentsHiuNotifyResp
    {
        public string requestId { get; set; }
        public string timestamp { get; set; }
        public Consent? consent { get; set; }
        public object? error { get; set; }
        public object? response { get; set; }
        public Resp? resp { get; set; }
    }

    public class Resp
    {
        public string requestId { get; set; }
    }

    public class HiuConsentDetailUpdate
    {
        public string consent_artefacts_id { get; set; }
        public string hip_id { get; set; }
        public string hip_name { get; set; }
        public string access_mode { get; set; }
        public DateTime? date_range_from { get; set; }
        public DateTime? date_range_to { get; set; }
        public DateTime? data_erase_at { get; set; }
        public string care_contexts { get; set; }
        public string hi_types { get; set; } // New property for hiTypes
    }
}

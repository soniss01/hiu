using Microsoft.EntityFrameworkCore;

namespace HIUServices.Entities.Requests.Patients
{
    [Keyless]
    public class PatientInfo
    {
        public string? requestId { get; set; }
        public string? timestamp { get; set; }
        public string? patient_id { get; set; }
        public string? patient_name { get; set; }
        public string? error_code { get; set; }
        public string? error_message { get; set; }
        public string? resp_requestId { get; set; }
    }
}

using HIUServices.Entities.Callbacks;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIUServices.Entities.Requests.Patients
{
    public class PatientFindInsert
    {
        public int? anm_id { get; set; }
        public string? anm_name { get; set; }
        public int? state_code { get; set; }
        public string? request_id { get; set; }
        public string? patient_id { get; set; }
        public string? abha_no { get; set; }
        public DateTime? dob { get; set; }
        public int? source_id { get;set; }
    }
}

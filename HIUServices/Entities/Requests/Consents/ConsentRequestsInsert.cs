using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIUServices.Entities.Requests.Consents
{
    public class ConsentRequestsInsert
    {
        public int anm_id { get; set; }
        public string anm_name { get; set; }
        public int state_code { get; set; }
        public string request_id { get; set; }
        public string patient_id { get; set; }
        public string patient_name { get; set; }
        public string hiu { get; set; }
        public string purpose_code { get; set; }
        public int requester_id { get; set; }
        public string hi_types { get; set; }
        public DateTime? permission_date_from { get; set; }
        public DateTime? permission_date_to { get; set; }
        public DateTime? data_erase_at { get; set; }
        public int source_id { get; set; }
        public string? abha_no { get; set; }
        public DateTime? dob { get; set; }

    }
}

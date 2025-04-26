using System.ComponentModel.DataAnnotations;

namespace HIUServices.Entities.Requests.Consents
{
    public class ConsentRequestDto
    {
        [Required]
        public int? anm_id { get; set; }
        public string? anm_name { get; set; }
        [Required]
        public int? state_code { get; set; }
        [Required]
        public string patient_id { get; set; }

        [Required]
        public string patient_name { get; set; }

        [Required]
        public string? abha_no { get; set; }

        [Required]
        public DateTime? dob { get; set; }

        [Required]
        public string purpose_text { get; set; }
        [Required]
        public string purpose_code { get; set; }
       
        [Required]
        public string[] hi_types { get; set; }
        [Required]
        public DateTime permission_date_from { get; set; }
        [Required]
        public DateTime permission_date_to { get; set; }
        [Required]
        public DateTime data_erase_at { get; set; }

        [Required]
        public int? source_id { get; set; }
    }

    public class ConsentRequestsDataDto
    {
        [Required]
        public int? anm_id { get; set; }

        [Required]
        public int? state_code { get; set; }
    }
}

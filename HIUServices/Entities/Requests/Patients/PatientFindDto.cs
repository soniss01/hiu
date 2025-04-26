using System.ComponentModel.DataAnnotations;

namespace HIUServices.Entities.Requests.Patients
{
    public class PatientRequestDto
    {
        [Required]
        public string patient_id { get; set; }

        [Required]
        public string? abha_no { get; set; }

        [Required]
        public DateTime? dob { get; set; }

        [Required]
        public int anm_id { get; set; }
        public string? anm_name { get; set; }
        [Required]
        public int state_code { get; set; }
        [Required]
        public int? source_id { get; set; }
    }
}

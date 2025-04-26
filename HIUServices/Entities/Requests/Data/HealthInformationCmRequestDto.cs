using System.ComponentModel.DataAnnotations;

namespace HIUServices.Entities.Requests
{
    public class HealthInformationCmRequestDto
    {
        [Required]
        public string consent_id { get; set; }
        [Required]
        public DateTime range_from { get; set; }
        [Required]
        public DateTime range_to { get; set; }
        [Required]
        public DateTime expiry { get; set; }
        [Required]
        public int source_id { get; set; }
    }

    public class HealthInformationDataReq
    {
        [Required]
        public string consent_id { get; set; }
       
    }
}

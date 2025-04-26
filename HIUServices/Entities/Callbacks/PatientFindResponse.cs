using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIUServices.Entities.Callbacks
{
    [Keyless]
    public class PatientFindResponse
    {
        public string? requestId { get; set; }
        public string? timestamp { get; set; }

        [NotMapped]
        public virtual Patient? patient { get; set; }

        [NotMapped]
        public virtual Error? error { get; set; }

        [NotMapped] 
        public virtual RequestResp? resp { get; set; }
    }

    [Keyless]
    public class Patient
    {
        public string? id { get; set; }
        public string? name { get; set; }
    }

    [Keyless]
    public class Error
    {
        public string? code { get; set; }
        public string? message { get; set; }
    }

    [Keyless]
    public class RequestResp
    {
        public string? requestId { get; set; }
    }
}

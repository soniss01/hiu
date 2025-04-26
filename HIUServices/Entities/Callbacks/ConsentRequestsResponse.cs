using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIUServices.Entities.Callbacks
{
    [Keyless]
    public class ConsentRequestsResponse
    {
        public string? requestId { get; set; }
        public string? timestamp { get; set; }

        [NotMapped]
        public ConsentRequest? consentRequest { get; set; }

        [NotMapped]
        public string? error { get; set; }

        [NotMapped] 
        public Response? resp { get; set; }
    }

    [Keyless]
    public class ConsentRequest
    {
        public string? id { get; set; }
    }

    [Keyless]
    public class Response
    {
        public string? requestId { get; set; }
    }
}

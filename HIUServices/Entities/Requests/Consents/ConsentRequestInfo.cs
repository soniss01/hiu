using Microsoft.EntityFrameworkCore;

namespace HIUServices.Entities.Requests.Consents
{
    [Keyless]
    public class ConsentRequestInfo
    {
        public string requestId { get; set; }
        public string timestamp { get; set; }
        public string consentRequest_id { get; set; }
        public string error { get; set; }
        public string resp_requestId { get; set; }
    }
}

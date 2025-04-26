using HIUServices.Entities.Requests.Data;
using Microsoft.EntityFrameworkCore;

namespace HIUServices.Entities.Callbacks
{
    [Keyless]
    public class HealthInformationHiuOnRequest
    {
        public string? requestId { get; set; }
        public string? timestamp { get; set; }
        public HiRequest? hiRequest { get; set; }
        public Errors? error { get; set; }
        public Responses? resp { get; set; }
    }

    [Keyless]
    public class HiRequest
    {
        public string? transactionId { get; set; }
        public string? sessionStatus { get; set; }
    }

    [Keyless]
    public class Errors
    {
        public string? code { get; set; }
        public string? message { get; set; }
    }

    [Keyless]
    public class Responses
    {
        public string? requestId { get; set; }
    }
}

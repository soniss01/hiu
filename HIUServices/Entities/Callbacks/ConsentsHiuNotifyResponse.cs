using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIUServices.Entities.Callbacks
{
    [Keyless]
    public class ConsentsHiuNotifyResponse
    {
        public string? requestId { get; set; }
        public string? timestamp { get; set; }
        public Notification? notification { get; set; }
    }
    [Keyless]
    public class Notification
    {
        public string? consentRequestId{ get; set; }
        public string? status { get; set; }

        public string? reason { get; set; }
        public ConsentArtefacts[]? consentArtefacts { get; set; }

    }
    [Keyless]
    public class ConsentArtefacts
    {
        public string? id { get; set; }
    }
}

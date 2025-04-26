using Microsoft.EntityFrameworkCore;

namespace HIUServices.Entities.Requests.Masters
{
    [Keyless]
    public class PurposeOfUse
    {
        public string? code { get; set; }
        public string? display { get; set; }
    }
}

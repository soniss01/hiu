using Microsoft.EntityFrameworkCore;

namespace HIUServices.Entities.Requests.Masters
{
    [Keyless]
    public class HiTypes
    {
        public string code { get; set; }
        public string display { get; set; }
    }
}

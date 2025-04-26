using HIUServices.Entities.Requests.Masters;

namespace HIUServices.Repostitories.Masters.Interfaces
{
    public interface IMasterService
    {
        Task<List<PurposeOfUse>> GetPurposeOfUse();
        Task<List<HiTypes>> GetHiTypes();
    }
}

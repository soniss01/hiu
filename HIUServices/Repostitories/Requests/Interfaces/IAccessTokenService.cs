namespace HIUServices.Repostitories.Requests.Interfaces
{
    public interface IAccessTokenService
    {
        Task<string> GetAccessToken();
    }
}

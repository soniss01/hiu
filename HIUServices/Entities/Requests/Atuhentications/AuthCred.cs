namespace HIUServices.Entities.Requests.Atuhentications
{
    public class AuthCred
    {
        public string clientId { get; set; }
        public string clientSecret { get; set; }
        public string grantType { get; set; }
    }
}

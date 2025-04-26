using HIUServices.Entities.Requests.Atuhentications;
using HIUServices.Repostitories.Requests.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System;

namespace HIUServices.Repostitories.Requests
{
    public class AccessTokenService : IAccessTokenService
    {
        private readonly IConfiguration configuration;
        private readonly string baseUrl;
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string xCmId;

        public AccessTokenService(IConfiguration configuration)
        {
            this.configuration = configuration;
            baseUrl = configuration.GetValue<string>("AbdmConfiguraton:accessTokenUrl");
            clientId = configuration.GetValue<string>("AbdmConfiguraton:clientId");
            clientSecret = configuration.GetValue<string>("AbdmConfiguraton:clientSecret");
            xCmId = configuration.GetValue<string>("AbdmConfiguraton:xcmid");
        }

        //public async Task<string> GetAccessToken()
        //{
        //var obj = new AuthCred()
        //    {
        //        clientId = clientId,
        //        clientSecret = clientSecret
        //    };

        //    var client = new HttpClient();
        //    var request = new HttpRequestMessage(HttpMethod.Post, baseUrl + "sessions");
        //    var serializeContent = System.Text.Json.JsonSerializer.Serialize(obj);
        //    var content = new StringContent(serializeContent, null, "application/json");
        //    request.Content = content;
        //    var response = await client.SendAsync(request);
        //    var result = await response.Content.ReadAsStringAsync();
        //    dynamic tokendata = JsonConvert.DeserializeObject(result);
        //    string token = tokendata.accessToken;
        //    return token;
        //}

        public async Task<string> GetAccessToken()
        {
            var obj = new AuthCred()
            {
                clientId = clientId,
                clientSecret = clientSecret,
                grantType = "client_credentials"
            };

            var client = new HttpClient();

            string requestId = Guid.NewGuid().ToString();
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            var request = new HttpRequestMessage(HttpMethod.Post, baseUrl)
            {
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json")
            };

            request.Headers.Add("REQUEST-ID", requestId);
            request.Headers.Add("TIMESTAMP", timestamp);
            request.Headers.Add("X-CM-ID", xCmId);

            var response = await client.SendAsync(request);

            var result = await response.Content.ReadAsStringAsync();
            dynamic tokendata = JObject.Parse(result);

            string token = tokendata.accessToken;

            return token;
        }
    }
}

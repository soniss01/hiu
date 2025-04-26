using HIUServices.DbContexts;
using HIUServices.Entities.Callbacks;
using HIUServices.Helpers;
using HIUServices.Repostitories.Callbacks.Interfaces;
using HIUServices.Repostitories.Requests.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using System.Text;

namespace HIUServices.Repostitories.Callbacks
{
    public class CallbackService : ICallbackService
    {
        private readonly IConfiguration configuration;
        private readonly CallbackDbContext _dbContext;
        private IAccessTokenService accessTokenService;
        private readonly string baseUrl;
        private readonly string XCMID;
        private readonly string HIU;
        private readonly string HIP;

        public CallbackService(IConfiguration configuration, CallbackDbContext dbContext, IAccessTokenService accessTokenService)
        {
            this.configuration = configuration;
            _dbContext = dbContext;
            this.accessTokenService = accessTokenService;
            baseUrl = this.configuration.GetValue<string>("AbdmConfiguraton:baseUrl");
            XCMID = this.configuration.GetValue<string>("AbdmConfiguraton:xcmid");
        }

        public async Task<int> PatientFind(PatientFindResponse obj)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@p_request_id", NpgsqlDbType.Text)  { Value = obj.resp.requestId != null ? obj.resp.requestId : (object)DBNull.Value },
                new NpgsqlParameter("@p_timestamp", NpgsqlDbType.Text) { Value = obj.timestamp != null ? obj.timestamp : (object)DBNull.Value },
                new NpgsqlParameter("@p_patient_name", NpgsqlDbType.Text) { Value = obj.patient?.name != null ? obj.patient.name : (object)DBNull.Value },
                new NpgsqlParameter("@p_error_code", NpgsqlDbType.Text) { Value = obj.error?.code != null ? obj.error.code : (object)DBNull.Value },
                new NpgsqlParameter("@p_error_message", NpgsqlDbType.Text) { Value = obj.error?.message != null ? obj.error.message : (object)DBNull.Value }
            };

            var result = await _dbContext.Database.ExecuteSqlRawAsync(
                "CALL public.sp_hiu_patient_find_update(@p_request_id, @p_timestamp, @p_patient_name, @p_error_code, @p_error_message)",
                parameters);
            return result;
        }

        public async Task<int> ConsentRequest(ConsentRequestsResponse consentRequests)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("@p_request_id", NpgsqlDbType.Text)  { Value = consentRequests.resp.requestId != null ? consentRequests.resp.requestId : (object)DBNull.Value },
                new NpgsqlParameter("@p_timestamp", NpgsqlDbType.Text) { Value = consentRequests.timestamp != null ? consentRequests.timestamp : (object)DBNull.Value },
                new NpgsqlParameter("@p_consent_request_id", NpgsqlDbType.Text) { Value = consentRequests.consentRequest?.id != null ? consentRequests.consentRequest.id : (object)DBNull.Value },
                new NpgsqlParameter("@p_error", NpgsqlDbType.Text) { Value = consentRequests.error != null ? consentRequests.error : (object)DBNull.Value },
               };

            var result = await _dbContext.Database.ExecuteSqlRawAsync(
                "CALL public.sp_hiu_consent_request_update(@p_request_id, @p_timestamp, @p_consent_request_id, @p_error)",
                parameters);

            return result;
        }

        public async Task<int> ConsentsHiuNotify(ConsentsHiuNotifyResponse response)
        {
            if (response.notification == null)
                throw new ArgumentException("Notification is missing from the response.");

            // Define the base parameters for the stored procedure call
            var baseParameters = new[]
            {
        new NpgsqlParameter("@p_request_id", NpgsqlDbType.Text) { Value = response.requestId ?? (object)DBNull.Value },
        new NpgsqlParameter("@p_timestamp", NpgsqlDbType.Text) { Value = response.timestamp ?? (object)DBNull.Value },
        new NpgsqlParameter("@p_consent_request_id", NpgsqlDbType.Text) { Value = response.notification.consentRequestId ?? (object)DBNull.Value },
        new NpgsqlParameter("@p_status", NpgsqlDbType.Text) { Value = response.notification.status ?? (object)DBNull.Value },
        new NpgsqlParameter("@p_reason", NpgsqlDbType.Text) { Value = response.notification.reason ?? (object)DBNull.Value }
    };

            int result = 0;

            if (response.notification.consentArtefacts != null && response.notification.consentArtefacts.Length > 0)
            {
                // Handle the case where consentArtefacts is not null or empty
                foreach (var artefact in response.notification.consentArtefacts)
                {
                    var parameters = new List<NpgsqlParameter>(baseParameters)
            {
                new NpgsqlParameter("@p_consent_artefacts_id", NpgsqlDbType.Text)
                {
                    Value = !string.IsNullOrEmpty(artefact.id) ? artefact.id : (object)DBNull.Value
                }
            };

                    result += await _dbContext.Database.ExecuteSqlRawAsync(
                        "CALL public.sp_hiu_consents_notify_insert(@p_request_id, @p_timestamp, @p_consent_request_id, @p_status, @p_reason, @p_consent_artefacts_id)",
                        parameters);
                    Console.WriteLine("consents notify inserted");

                    // Call the external API for each artefact
                    Console.WriteLine("FetchConsentData called");
                    await FetchConsentData(artefact.id);
                }
            }
            else
            {
                // Handle the case where consentArtefacts is null or empty
                var parameters = new List<NpgsqlParameter>(baseParameters)
        {
            new NpgsqlParameter("@p_consent_artefacts_id", NpgsqlDbType.Text)
            {
                Value = DBNull.Value
            }
        };

                result = await _dbContext.Database.ExecuteSqlRawAsync(
                    "CALL public.sp_hiu_consents_notify_insert(@p_request_id, @p_timestamp, @p_consent_request_id, @p_status, @p_reason, @p_consent_artefacts_id)",
                    parameters);
                Console.WriteLine("consents notify inserted");
            }

            Console.WriteLine("result " + result);
            return result;
        }


        //public async Task<int> ConsentsDetailsUpdate(HiuConsentDetailUpdate obj)
        //{
        //    var parameters = new[]
        //    {
        //       new NpgsqlParameter("@p_consent_artefacts_id", NpgsqlDbType.Varchar) { Value = obj.consent_artefacts_id ?? (object)DBNull.Value },
        //       new NpgsqlParameter("@p_hip_id", NpgsqlDbType.Varchar) { Value = obj.hip_id ?? (object)DBNull.Value },
        //       new NpgsqlParameter("@p_hip_name", NpgsqlDbType.Varchar) { Value = obj.hip_name ?? (object)DBNull.Value },
        //       new NpgsqlParameter("@p_access_mode", NpgsqlDbType.Varchar) { Value = obj.access_mode ?? (object)DBNull.Value },
        //       new NpgsqlParameter("@p_date_range_from", NpgsqlDbType.Timestamp) { Value = obj.date_range_from },
        //       new NpgsqlParameter("@p_date_range_to", NpgsqlDbType.Timestamp) { Value = obj.date_range_to },
        //       new NpgsqlParameter("@p_data_erase_at", NpgsqlDbType.Timestamp) { Value = obj.data_erase_at },
        //       new NpgsqlParameter("@p_care_contexts", NpgsqlDbType.Text) { Value = obj.care_contexts ?? (object)DBNull.Value },
        //       new NpgsqlParameter("@p_hi_types", NpgsqlDbType.Varchar) { Value = obj.hi_types ?? (object)DBNull.Value }
        //    };

        //    var result = await _dbContext.Database.ExecuteSqlRawAsync(
        //        "CALL public.sp_hiu_consents_notify_update(@p_consent_artefacts_id, @p_hip_id, @p_hip_name, @p_access_mode, @p_date_range_from, @p_date_range_to, @p_data_erase_at, @p_care_contexts,@p_hi_types)",
        //        parameters);

        //    return result;
        //}

        public async Task<int> ConsentsDetailsUpdate(HiuConsentDetailUpdate obj)
        {
            var parameters = new[]
            {
        new NpgsqlParameter("@p_consent_artefacts_id", NpgsqlDbType.Varchar) { Value = obj.consent_artefacts_id ?? (object)DBNull.Value },
        new NpgsqlParameter("@p_hip_id", NpgsqlDbType.Varchar) { Value = obj.hip_id ?? (object)DBNull.Value },
        new NpgsqlParameter("@p_hip_name", NpgsqlDbType.Varchar) { Value = obj.hip_name ?? (object)DBNull.Value },
        new NpgsqlParameter("@p_access_mode", NpgsqlDbType.Varchar) { Value = obj.access_mode ?? (object)DBNull.Value },
        new NpgsqlParameter("@p_date_range_from", NpgsqlDbType.Timestamp) { Value = Utilities.ConvertToLocalTime(obj.date_range_from) },
        new NpgsqlParameter("@p_date_range_to", NpgsqlDbType.Timestamp) { Value = Utilities.ConvertToLocalTime(obj.date_range_to) },
        new NpgsqlParameter("@p_data_erase_at", NpgsqlDbType.Timestamp) { Value = Utilities.ConvertToLocalTime(obj.data_erase_at) },
        new NpgsqlParameter("@p_care_contexts", NpgsqlDbType.Text) { Value = obj.care_contexts ?? (object)DBNull.Value },
        new NpgsqlParameter("@p_hi_types", NpgsqlDbType.Varchar) { Value = obj.hi_types ?? (object)DBNull.Value }
    };

            var result = await _dbContext.Database.ExecuteSqlRawAsync(
                "CALL public.sp_hiu_consents_notify_update(@p_consent_artefacts_id, @p_hip_id, @p_hip_name, @p_access_mode, @p_date_range_from, @p_date_range_to, @p_data_erase_at, @p_care_contexts, @p_hi_types)",
                parameters);

            return result;
        }

        private async Task<dynamic> FetchConsentData(string? consentId)
        {
            Console.WriteLine("consentId " + consentId);
            Guid requestId = Guid.NewGuid();
            var requestData = new
            {
                requestId = requestId.ToString(),
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                consentId = consentId
            };
            string accessToken = await accessTokenService.GetAccessToken();
            HttpClientHandler handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
            };
            var client = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Post, baseUrl + "consents/fetch");
            request.Headers.Add("X-CM-ID", XCMID);
            request.Headers.Add("Authorization", "Bearer " + accessToken);
            var serializeContent = System.Text.Json.JsonSerializer.Serialize(requestData);
            Console.WriteLine(serializeContent);
            request.Content = new StringContent(serializeContent, Encoding.UTF8, "application/json");
            var response = await client.SendAsync(request);
            Console.WriteLine(response.StatusCode);
            return response;
        }

        public async Task<int> HealthInformationHiuOnResponse(HealthInformationHiuOnRequest response)
        {
            var parameters = new[]
            {
        new NpgsqlParameter("@p_transaction_id", NpgsqlDbType.Text) { Value = response.hiRequest?.transactionId ?? (object)DBNull.Value },
        new NpgsqlParameter("@p_session_status", NpgsqlDbType.Text) { Value = response.hiRequest?.sessionStatus ?? (object)DBNull.Value },
        new NpgsqlParameter("@p_error_code", NpgsqlDbType.Text) { Value = response.error?.code ?? (object)DBNull.Value },
        new NpgsqlParameter("@p_error_message", NpgsqlDbType.Text) { Value = response.error?.message ?? (object)DBNull.Value },
        new NpgsqlParameter("@p_request_id", NpgsqlDbType.Text) { Value = response.resp?.requestId ?? (object)DBNull.Value }
    };

            var result = await _dbContext.Database.ExecuteSqlRawAsync(
                "CALL public.sp_hi_hiu_on_request_update(@p_transaction_id, @p_session_status, @p_error_code, @p_error_message, @p_request_id)",
                parameters);

            return result;
        }

        public async Task<int> HealthInformationTransfer(HIData response)
        {
            var parameters = new[]
            {
            new NpgsqlParameter("@p_expiry_date", NpgsqlDbType.Timestamp) { Value =Utilities.ConvertToLocalTime(response.expiry) },
            new NpgsqlParameter("@p_transaction_id", NpgsqlDbType.Text) { Value = response.transactionId ?? (object)DBNull.Value },
            new NpgsqlParameter("@p_sender_public_key", NpgsqlDbType.Text) { Value = response.publicKey ?? (object)DBNull.Value },
            new NpgsqlParameter("@p_sender_nonce", NpgsqlDbType.Text) { Value = response.nonce ?? (object)DBNull.Value },
            new NpgsqlParameter("@p_response_data", NpgsqlDbType.Text) { Value = response.content != null ? response.content : (object)DBNull.Value },
            new NpgsqlParameter("@p_care_context", NpgsqlDbType.Text) { Value = response.care_context != null ? response.care_context : (object)DBNull.Value },
            };

            var result = await _dbContext.Database.ExecuteSqlRawAsync(
                "CALL public.sp_hi_data_insert( @p_expiry_date,@p_transaction_id,@p_sender_public_key, @p_sender_nonce, @p_response_data,@p_care_context)",
                parameters);

            return result;
        }
    }
}

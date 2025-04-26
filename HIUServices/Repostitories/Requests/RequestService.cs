using HIUServices.DbContexts;
using HIUServices.Entities.Requests.Consents;
using HIUServices.Entities.Requests.Patients;
using HIUServices.Repostitories.Requests.Interfaces;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Data;
using NpgsqlTypes;
using HIUServices.Entities.Requests.Data;
using HIUServices.Helpers;
using Newtonsoft.Json;

namespace HIUServices.Repostitories.Requests
{
    public class RequestService : IRequestService
    {
        private readonly IConfiguration configuration;
        private readonly RequestDbContext dbContext;
        private IAccessTokenService accessTokenService;
        private readonly string baseUrl;
        private readonly string XCMID;
        private readonly string HIU;
        private readonly string HIP;

        public RequestService(IConfiguration configuration, RequestDbContext dbContext, IAccessTokenService accessTokenService)
        {
            this.configuration = configuration;
            this.dbContext = dbContext;
            this.accessTokenService = accessTokenService;
            baseUrl = configuration.GetValue<string>("AbdmConfiguraton:baseUrl");
            XCMID = configuration.GetValue<string>("AbdmConfiguraton:xcmid");
        }

        public async Task<DataTable> GetPatientByRequestId(string requestId)
        {
            var request_Id = new NpgsqlParameter("@p_request_id", NpgsqlDbType.Varchar);
            request_Id.Value = requestId;

            using (var command = dbContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "SELECT * FROM sp_hiu_patient_find_select(@p_request_id)";
                command.CommandType = CommandType.Text;
                command.Parameters.Add(request_Id);

                await dbContext.Database.OpenConnectionAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Load(reader);
                    return dataTable;
                }
            }
        }

        public async Task<DataTable> GetConsentByRequestId(string requestId)
        {
            var request_Id = new NpgsqlParameter("@p_request_id", requestId);

            using (var command = dbContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "SELECT* FROM sp_hiu_consent_requests_select(@p_request_id)";
                command.CommandType = CommandType.Text;
                command.Parameters.Add(request_Id);

                dbContext.Database.OpenConnection();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Load(reader);
                    return dataTable;
                }
            }
        }

        public async Task<object[]> GetHIDataByConsentId(string consentId)
        {
            var consentIdParam = new NpgsqlParameter("@consent_id_param", consentId);

            using (var command = dbContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "SELECT * FROM get_joined_data_by_consent(@consent_id_param)";
                command.CommandType = CommandType.Text;
                command.Parameters.Add(consentIdParam);

                await dbContext.Database.OpenConnectionAsync();

                try
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        var healthDataList = new List<object>();

                        foreach (DataRow row in dataTable.Rows)
                        {
                            var xorOfRandoms = DHKeyExchangeCrypto.XorOfRandom(DHKeyExchangeCrypto.GetByteFromBase64(Convert.ToString(row["sender_nonce"])), DHKeyExchangeCrypto.GetByteFromBase64(Convert.ToString(row["receiver_nonce"]))).ToArray();
                            var healthData = DHKeyExchangeCrypto.DecryptString(row["response_data"] as string,
                                                                               xorOfRandoms,
                                                                               row["receiver_private_key"] as string,
                                                                               row["sender_public_key"] as string);

                            if (!string.IsNullOrEmpty(healthData))
                            {
                                dynamic json = JsonConvert.DeserializeObject(healthData);
                                healthDataList.Add(json);
                            }
                        }
                        return healthDataList.ToArray();
                    }
                }
                finally
                {
                    await dbContext.Database.CloseConnectionAsync();
                }
            }
        }

        public async Task<int> InsertPatientFind(PatientFindInsert obj)
        {
            var parameter = new List<NpgsqlParameter>();
            parameter.Add(new NpgsqlParameter("@p_anm_id", obj.anm_id));
            parameter.Add(new NpgsqlParameter("@p_anm_name", obj.anm_name ?? (object)DBNull.Value));
            parameter.Add(new NpgsqlParameter("@p_state_code", obj.state_code));
            parameter.Add(new NpgsqlParameter("@p_patient_id", obj.patient_id ?? (object)DBNull.Value));
            parameter.Add(new NpgsqlParameter("@p_request_id", obj.request_id ?? (object)DBNull.Value));
            parameter.Add(new NpgsqlParameter("@p_source_id", obj.source_id ?? (object)DBNull.Value));
            parameter.Add(new NpgsqlParameter("@p_abha_number", obj.abha_no ?? (object)DBNull.Value));
            parameter.Add(new NpgsqlParameter("@p_dob", obj.dob ?? (object)DBNull.Value));

            var result = await Task.Run(() => dbContext.Database
               .ExecuteSqlRawAsync(@"CALL public.sp_hiu_patient_find_insert(@p_anm_id, @p_anm_name, @p_state_code, @p_patient_id, @p_request_id,@p_source_id,@p_abha_number,@p_dob)", parameter.ToArray()));

            return result;
        }

        public async Task<int> InsertConsentRequest(ConsentRequestsInsert consentRequests)
        {
            var parameters = new List<NpgsqlParameter>
    {
        new NpgsqlParameter("@p_anm_id", NpgsqlDbType.Integer) { Value = consentRequests.anm_id },
        new NpgsqlParameter("@p_anm_name", NpgsqlDbType.Varchar) { Value = consentRequests.anm_name ?? (object)DBNull.Value },
        new NpgsqlParameter("@p_state_code", NpgsqlDbType.Integer) { Value = consentRequests.state_code },
        new NpgsqlParameter("@p_patient_id", NpgsqlDbType.Varchar) { Value = consentRequests.patient_id ?? (object)DBNull.Value },
        new NpgsqlParameter("@p_patient_name", NpgsqlDbType.Varchar) { Value = consentRequests.patient_name ?? (object)DBNull.Value },
        new NpgsqlParameter("@p_hiu", NpgsqlDbType.Varchar) { Value = consentRequests.hiu ?? (object)DBNull.Value },
        new NpgsqlParameter("@p_consent_purpose", NpgsqlDbType.Varchar) { Value = consentRequests.purpose_code ?? (object)DBNull.Value },
        new NpgsqlParameter("@p_hi_types", NpgsqlDbType.Varchar) { Value = consentRequests.hi_types ?? (object)DBNull.Value },
        new NpgsqlParameter("@p_requester_id", NpgsqlDbType.Integer) { Value = consentRequests.requester_id },
        new NpgsqlParameter("@p_permission_date_from", NpgsqlDbType.Timestamp) { Value = consentRequests.permission_date_from },
        new NpgsqlParameter("@p_permission_date_to", NpgsqlDbType.Timestamp) { Value = consentRequests.permission_date_to },
        new NpgsqlParameter("@p_data_erase_at", NpgsqlDbType.Timestamp) { Value = consentRequests.data_erase_at },
        new NpgsqlParameter("@p_request_id", NpgsqlDbType.Varchar) { Value = consentRequests.request_id },
        new NpgsqlParameter("@p_source_id", NpgsqlDbType.Integer) { Value = consentRequests.source_id },
        new NpgsqlParameter("@p_abha_number", NpgsqlDbType.Varchar) { Value = consentRequests.abha_no },
        new NpgsqlParameter("@p_dob", NpgsqlDbType.Timestamp) { Value = consentRequests.dob },
        new NpgsqlParameter("@p_result", NpgsqlDbType.Integer) { Direction = ParameterDirection.Output }
    };

            using (var command = new NpgsqlCommand("public.sp_hiu_consent_request_insert", dbContext.Database.GetDbConnection() as NpgsqlConnection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddRange(parameters.ToArray());

                await command.Connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                await command.Connection.CloseAsync();

                // Retrieve the result from the output parameter
                var result = (int)command.Parameters["@p_result"].Value;
                return result;
            }
        }


        //    public async Task<int> InsertConsentRequest(ConsentRequestsInsert consentRequests)
        //    {
        //        var parameters = new List<NpgsqlParameter>
        //{
        //    new NpgsqlParameter("@p_anm_id", NpgsqlDbType.Integer) { Value = consentRequests.anm_id },
        //    new NpgsqlParameter("@p_anm_name", NpgsqlDbType.Varchar) { Value = consentRequests.anm_name ?? (object)DBNull.Value },
        //    new NpgsqlParameter("@p_state_code", NpgsqlDbType.Integer) { Value = consentRequests.state_code },
        //    new NpgsqlParameter("@p_patient_id", NpgsqlDbType.Varchar) { Value = consentRequests.patient_id ?? (object)DBNull.Value },
        //    new NpgsqlParameter("@p_patient_name", NpgsqlDbType.Varchar) { Value = consentRequests.patient_name ?? (object)DBNull.Value },
        //    new NpgsqlParameter("@p_hiu", NpgsqlDbType.Varchar) { Value = consentRequests.hiu ?? (object)DBNull.Value },
        //    new NpgsqlParameter("@p_consent_purpose", NpgsqlDbType.Varchar) { Value = consentRequests.purpose_code ?? (object)DBNull.Value },
        //    new NpgsqlParameter("@p_hi_types", NpgsqlDbType.Varchar) { Value = consentRequests.hi_types ?? (object)DBNull.Value },
        //    new NpgsqlParameter("@p_requester_id", NpgsqlDbType.Integer) { Value = consentRequests.requester_id },
        //    new NpgsqlParameter("@p_permission_date_from", NpgsqlDbType.Timestamp) { Value = consentRequests.permission_date_from },
        //    new NpgsqlParameter("@p_permission_date_to", NpgsqlDbType.Timestamp) { Value = consentRequests.permission_date_to },
        //    new NpgsqlParameter("@p_data_erase_at", NpgsqlDbType.Timestamp) { Value = consentRequests.data_erase_at },
        //    new NpgsqlParameter("@p_request_id", NpgsqlDbType.Varchar) { Value = consentRequests.request_id },
        //    new NpgsqlParameter("@p_source_id", NpgsqlDbType.Integer) { Value = consentRequests.source_id },
        //    new NpgsqlParameter("@p_result", NpgsqlDbType.Integer) { Direction = ParameterDirection.Output }
        //};

        //        await dbContext.Database.ExecuteSqlRawAsync(
        //            @"CALL public.sp_hiu_consent_request_insert(
        //            @p_anm_id,
        //            @p_anm_name,
        //            @p_state_code,
        //            @p_patient_id,
        //            @p_patient_name,
        //            @p_hiu,
        //            @p_consent_purpose,
        //            @p_hi_types,
        //            @p_requester_id,
        //            @p_permission_date_from,
        //            @p_permission_date_to,
        //            @p_data_erase_at,
        //            @p_request_id,
        //            @p_source_id,
        //            @p_result)",
        //            parameters.ToArray()
        //        );

        //        // Retrieve the result from the stored procedure
        //        var result = (int)parameters.First(p => p.ParameterName == "@p_result").Value;
        //        return result;
        //    }

        public async Task<dynamic> PatientsFind(PatientFind obj)
        {
            string accessToken = await accessTokenService.GetAccessToken();
            HttpClientHandler handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
            };
            var client = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Post, baseUrl + "patients/find");
            request.Headers.Add("X-CM-ID", XCMID);
            request.Headers.Add("Authorization", "Bearer " + accessToken);
            var serializeContent = System.Text.Json.JsonSerializer.Serialize(obj);
            request.Content = new StringContent(serializeContent, Encoding.UTF8, "application/json");
            var response = await client.SendAsync(request);
            return response;
        }

        public async Task<dynamic> ConsentRequests(HIUServices.Entities.Requests.Consents.ConsentRequest obj)
        {
            string accessToken = await accessTokenService.GetAccessToken();
            HttpClientHandler handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
            };
            var client = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Post, baseUrl + "consent-requests/init");
            request.Headers.Add("X-CM-ID", XCMID);
            request.Headers.Add("Authorization", "Bearer " + accessToken);
            var serializeContent = System.Text.Json.JsonSerializer.Serialize(obj);
            request.Content = new StringContent(serializeContent, Encoding.UTF8, "application/json");
            var response = await client.SendAsync(request);
            return response;
        }

        //public async Task<DataTable> ConsentRequestsData(ConsentRequestsDataDto consentRequestDataDto)
        //{
        //    var anmIdParam = new NpgsqlParameter("@anm_id", NpgsqlDbType.Integer);
        //    anmIdParam.Value = consentRequestDataDto.anm_id;

        //    var stateCodeParam = new NpgsqlParameter("@state_code", NpgsqlDbType.Integer);
        //    stateCodeParam.Value = consentRequestDataDto.state_code;

        //    using (var command = dbContext.Database.GetDbConnection().CreateCommand())
        //    {
        //        command.CommandText = "SELECT * FROM public.sp_hiu_consent_requests_data(@anm_id, @state_code)";
        //        command.CommandType = CommandType.Text;
        //        command.Parameters.Add(anmIdParam);
        //        command.Parameters.Add(stateCodeParam);

        //        await dbContext.Database.OpenConnectionAsync();

        //        using (var reader = await command.ExecuteReaderAsync())
        //        {
        //            DataTable dataTable = new DataTable();
        //            dataTable.Load(reader);
        //            return dataTable;
        //        }
        //    }
        //}

        //    public async Task<List<ParentConsent>> ConsentRequestsData(ConsentRequestsDataDto consentRequestDataDto)
        //    {
        //        var anmIdParam = new NpgsqlParameter("@anm_id", NpgsqlDbType.Integer) { Value = consentRequestDataDto.anm_id };
        //        var stateCodeParam = new NpgsqlParameter("@state_code", NpgsqlDbType.Integer) { Value = consentRequestDataDto.state_code };

        //        using (var command = dbContext.Database.GetDbConnection().CreateCommand())
        //        {
        //            command.CommandText = "SELECT * FROM public.sp_hiu_consent_requests_data(@anm_id, @state_code)";
        //            command.CommandType = CommandType.Text;
        //            command.Parameters.Add(anmIdParam);
        //            command.Parameters.Add(stateCodeParam);

        //            await dbContext.Database.OpenConnectionAsync();

        //            using (var reader = await command.ExecuteReaderAsync())
        //            {
        //                var parentConsents = new List<ParentConsent>();

        //                while (await reader.ReadAsync())
        //                {
        //                    string patientId = reader["patient_id"].ToString();
        //                    DateTime consentCreatedOnDate = Convert.ToDateTime(reader["consent_created_on"]).Date;

        //                    var parent = parentConsents
        //.FirstOrDefault(p => p.patient_id == patientId && p.consent_created_on.HasValue && p.consent_created_on.Value.Date == consentCreatedOnDate);


        //                    if (parent == null)
        //                    {
        //                        parent = new ParentConsent
        //                        {
        //                            patient_id = patientId,
        //                            patient_name = reader["patient_name"].ToString(),
        //                            consent_request_id = reader["consent_request_id"].ToString(),
        //                            consent_created_on = Convert.ToDateTime(reader["consent_created_on"]),
        //                            consents = new List<ChildConsent>()
        //                        };
        //                        parentConsents.Add(parent);
        //                    }

        //                    var child = new ChildConsent
        //                    {
        //                        status = reader["status"] != DBNull.Value ? reader["status"].ToString() : null,
        //                        reason = reader["reason"] != DBNull.Value ? reader["reason"].ToString() : null,
        //                        consent_artefacts_id = reader["consent_artefacts_id"] != DBNull.Value ? reader["consent_artefacts_id"].ToString() : null,
        //                        hip_name = reader["hip_name"] != DBNull.Value ? reader["hip_name"].ToString() : null,
        //                        date_range_from = reader["date_range_from"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["date_range_from"]) : null,
        //                        date_range_to = reader["date_range_to"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["date_range_to"]) : null,
        //                        data_erase_at = reader["data_erase_at"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["data_erase_at"]) : null,
        //                        artefact_created_on = reader["artefact_created_on"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["artefact_created_on"]) : null
        //                    };

        //                    parent.consents.Add(child);
        //                }

        //                return parentConsents;
        //            }
        //        }
        //    }



        //public async Task<List<ParentConsent>> ConsentRequestsData(ConsentRequestsDataDto consentRequestDataDto)
        //{
        //    var anmIdParam = new NpgsqlParameter("@p_anm_id", NpgsqlDbType.Integer) { Value = consentRequestDataDto.anm_id };
        //    var stateCodeParam = new NpgsqlParameter("@p_state_code", NpgsqlDbType.Integer) { Value = consentRequestDataDto.state_code };

        //    using (var command = dbContext.Database.GetDbConnection().CreateCommand())
        //    {
        //        command.CommandText = "SELECT * FROM public.sp_hiu_consent_requests_data(@p_anm_id, @p_state_code)";
        //        command.CommandType = CommandType.Text;
        //        command.Parameters.Add(anmIdParam);
        //        command.Parameters.Add(stateCodeParam);

        //        await dbContext.Database.OpenConnectionAsync();

        //        using (var reader = await command.ExecuteReaderAsync())
        //        {
        //            var parentConsents = new List<ParentConsent>();
        //            var consentLookup = new Dictionary<(string patientId, string consentArtefactsId), ParentConsent>();

        //            while (await reader.ReadAsync())
        //            {
        //                string patientId = reader["patient_id"].ToString();
        //                string consentRequestId = reader["consent_request_id"].ToString();
        //                DateTime consentCreatedOnDate = Convert.ToDateTime(reader["consent_created_on"]).Date;

        //                var consentArtefactsId = reader["consent_artefacts_id"] != DBNull.Value ? reader["consent_artefacts_id"].ToString() : null;

        //                var parentKey = (patientId, consentArtefactsId);

        //                if (!consentLookup.TryGetValue(parentKey, out var parent))
        //                {
        //                    parent = new ParentConsent
        //                    {
        //                        patient_id = patientId,
        //                        patient_name = reader["patient_name"].ToString(),
        //                        consent_request_id = consentRequestId,
        //                        consent_created_on = reader["consent_created_on"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["consent_created_on"]) : null,
        //                        requested_date_from = reader["requested_date_from"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["requested_date_from"]) : null,
        //                        requested_date_to = reader["requested_date_to"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["requested_date_to"]) : null,
        //                        requested_expiry_date = reader["requested_expiry_date"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["requested_expiry_date"]) : null,
        //                        consents = new List<ChildConsent>(),
        //                        requested_hi_types = reader["requested_hi_type_names"] != DBNull.Value ? (string)reader["requested_hi_type_names"] : null
        //                    };

        //                    parentConsents.Add(parent);
        //                    consentLookup[parentKey] = parent;
        //                }

        //                var child = new ChildConsent
        //                {
        //                    status = reader["status"] != DBNull.Value ? reader["status"].ToString() : null,
        //                    reason = reader["reason"] != DBNull.Value ? reader["reason"].ToString() : null,
        //                    consent_artefacts_id = reader["consent_artefacts_id"] != DBNull.Value ? reader["consent_artefacts_id"].ToString() : null,
        //                    hip_id = reader["hip_id"] != DBNull.Value ? reader["hip_id"].ToString() : null,
        //                    hip_name = reader["hip_name"] != DBNull.Value ? reader["hip_name"].ToString() : null,
        //                    access_mode = reader["access_mode"] != DBNull.Value ? reader["access_mode"].ToString() : null,
        //                    date_range_from = reader["date_range_from"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["date_range_from"]) : null,
        //                    date_range_to = reader["date_range_to"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["date_range_to"]) : null,
        //                    data_erase_at = reader["data_erase_at"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["data_erase_at"]) : null,
        //                    artefact_created_on = reader["artefact_created_on"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["artefact_created_on"]) : null,
        //                    granted_hi_types = reader["granted_hi_type_names"] != DBNull.Value ? (string)reader["granted_hi_type_names"] : null
        //                };

        //                parent.consents.Add(child);
        //            }

        //            return parentConsents;
        //        }
        //    }
        //}


        //public async Task<List<ParentConsent>> ConsentRequestsData(ConsentRequestsDataDto consentRequestDataDto)
        //{
        //    var anmIdParam = new NpgsqlParameter("@p_anm_id", NpgsqlDbType.Integer) { Value = consentRequestDataDto.anm_id };
        //    var stateCodeParam = new NpgsqlParameter("@p_state_code", NpgsqlDbType.Integer) { Value = consentRequestDataDto.state_code };

        //    using (var command = dbContext.Database.GetDbConnection().CreateCommand())
        //    {
        //        command.CommandText = "SELECT * FROM public.sp_hiu_consent_requests_data(@p_anm_id, @p_state_code)";
        //        command.CommandType = CommandType.Text;
        //        command.Parameters.Add(anmIdParam);
        //        command.Parameters.Add(stateCodeParam);

        //        await dbContext.Database.OpenConnectionAsync();

        //        using (var reader = await command.ExecuteReaderAsync())
        //        {
        //            var parentConsents = new List<ParentConsent>();
        //            var consentLookup = new Dictionary<(string patientId, string consentRequestId), ParentConsent>();

        //            while (await reader.ReadAsync())
        //            {
        //                string patientId = reader["patient_id"].ToString();
        //                string consentRequestId = reader["consent_request_id"].ToString();

        //                var consentCreatedOn = ParseDateTime(reader["consent_created_on"]);
        //                var requestedDateFrom = ParseDateTime(reader["requested_date_from"]);
        //                var requestedDateTo = ParseDateTime(reader["requested_date_to"]);
        //                var requestedExpiryDate = ParseDateTime(reader["requested_expiry_date"]);

        //                var consentArtefactsId = reader["consent_artefacts_id"] != DBNull.Value ? reader["consent_artefacts_id"].ToString() : null;
        //                var dateRangeFrom = ParseDateTime(reader["date_range_from"]);
        //                var dateRangeTo = ParseDateTime(reader["date_range_to"]);
        //                var dataEraseAt = ParseDateTime(reader["data_erase_at"]);
        //                var artefactCreatedOn = ParseDateTime(reader["artefact_created_on"]);
        //                var grantedHiTypes = reader["granted_hi_type_names"] != DBNull.Value ? reader["granted_hi_type_names"].ToString() : null;
        //                var requestedHiTypes = reader["requested_hi_type_names"] != DBNull.Value ? reader["requested_hi_type_names"].ToString() : null;

        //                var parentKey = (patientId, consentRequestId);

        //                if (!consentLookup.TryGetValue(parentKey, out var parent))
        //                {
        //                    parent = new ParentConsent
        //                    {
        //                        patient_id = patientId,
        //                        patient_name = reader["patient_name"].ToString(),
        //                        consent_request_id = consentRequestId,
        //                        consent_created_on = consentCreatedOn,
        //                        requested_date_from = requestedDateFrom,
        //                        requested_date_to = requestedDateTo,
        //                        requested_expiry_date = requestedExpiryDate,
        //                        consents = new List<ChildConsent>(),
        //                        requested_hi_types = requestedHiTypes
        //                    };

        //                    parentConsents.Add(parent);
        //                    consentLookup[parentKey] = parent;
        //                }

        //                var child = new ChildConsent
        //                {
        //                    status = reader["status"] != DBNull.Value ? reader["status"].ToString() : null,
        //                    reason = reader["reason"] != DBNull.Value ? reader["reason"].ToString() : null,
        //                    consent_artefacts_id = reader["consent_artefacts_id"] != DBNull.Value ? reader["consent_artefacts_id"].ToString() : null,
        //                    hip_id = reader["hip_id"] != DBNull.Value ? reader["hip_id"].ToString() : null,
        //                    hip_name = reader["hip_name"] != DBNull.Value ? reader["hip_name"].ToString() : null,
        //                    access_mode = reader["access_mode"] != DBNull.Value ? reader["access_mode"].ToString() : null,
        //                    date_range_from = dateRangeFrom,
        //                    date_range_to = dateRangeTo,
        //                    data_erase_at = dataEraseAt,
        //                    artefact_created_on = artefactCreatedOn,
        //                    granted_hi_types = grantedHiTypes
        //                };

        //                parent.consents.Add(child);
        //            }

        //            return parentConsents;
        //        }
        //    }
        //}


        public async Task<List<ParentConsent>> ConsentRequestsData(ConsentRequestsDataDto consentRequestDataDto)
        {
            var anmIdParam = new NpgsqlParameter("@p_anm_id", NpgsqlDbType.Integer) { Value = consentRequestDataDto.anm_id };
            var stateCodeParam = new NpgsqlParameter("@p_state_code", NpgsqlDbType.Integer) { Value = consentRequestDataDto.state_code };

            using (var command = dbContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "SELECT * FROM public.sp_hiu_consent_requests_data(@p_anm_id, @p_state_code)";
                command.CommandType = CommandType.Text;
                command.Parameters.Add(anmIdParam);
                command.Parameters.Add(stateCodeParam);

                await dbContext.Database.OpenConnectionAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    var parentConsents = new List<ParentConsent>();
                    var consentLookup = new Dictionary<(string patientId, string consentRequestId), ParentConsent>();

                    while (await reader.ReadAsync())
                    {
                        string patientId = reader["patient_id"].ToString();
                        string consentRequestId = reader["consent_request_id"].ToString();

                        // Handle date-time values directly
                        DateTime? dob = reader["dob"] != DBNull.Value ? (DateTime?)reader["dob"] : null;
                        DateTime? consentRequestedOn = reader["consent_requested_on"] != DBNull.Value ? (DateTime?)reader["consent_requested_on"] : null;
                        DateTime? requestedDateFrom = reader["requested_date_from"] != DBNull.Value ? (DateTime?)reader["requested_date_from"] : null;
                        DateTime? requestedDateTo = reader["requested_date_to"] != DBNull.Value ? (DateTime?)reader["requested_date_to"] : null;
                        DateTime? requestedExpiryDate = reader["requested_expiry_date"] != DBNull.Value ? (DateTime?)reader["requested_expiry_date"] : null;

                        string consentArtefactsId = reader["consent_artefacts_id"] != DBNull.Value ? reader["consent_artefacts_id"].ToString() : null;
                        DateTime? dateRangeFrom = reader["date_range_from"] != DBNull.Value ? (DateTime?)reader["date_range_from"] : null;
                        DateTime? dateRangeTo = reader["date_range_to"] != DBNull.Value ? (DateTime?)reader["date_range_to"] : null;
                        DateTime? dataEraseAt = reader["data_erase_at"] != DBNull.Value ? (DateTime?)reader["data_erase_at"] : null;
                        DateTime? consentRespondedOn = reader["consent_responded_on"] != DBNull.Value ? (DateTime?)reader["consent_responded_on"] : null;
                        string grantedHiTypes = reader["granted_hi_type_names"] != DBNull.Value ? reader["granted_hi_type_names"].ToString() : null;
                        string requestedHiTypes = reader["requested_hi_type_names"] != DBNull.Value ? reader["requested_hi_type_names"].ToString() : null;

                        var parentKey = (patientId, consentRequestId);

                        if (!consentLookup.TryGetValue(parentKey, out var parent))
                        {
                            parent = new ParentConsent
                            {
                                patient_id = patientId,
                                patient_name = reader["patient_name"].ToString(),
                                abha_number= reader["abha_number"].ToString(),
                                dob=dob,
                                consent_request_id = consentRequestId,
                                consent_requested_on = consentRequestedOn,
                                requested_date_from = requestedDateFrom,
                                requested_date_to = requestedDateTo,
                                requested_expiry_date = requestedExpiryDate,
                                consents = new List<ChildConsent>(),
                                requested_hi_types = requestedHiTypes,
                                status = reader["status"] != DBNull.Value ? reader["status"].ToString() : null
                            };

                            parentConsents.Add(parent);
                            consentLookup[parentKey] = parent;
                        }

                        var child = new ChildConsent
                        {
                            status = reader["status"] != DBNull.Value ? reader["status"].ToString() : null,
                            reason = reader["reason"] != DBNull.Value ? reader["reason"].ToString() : null,
                            consent_artefacts_id = consentArtefactsId,
                            hip_id = reader["hip_id"] != DBNull.Value ? reader["hip_id"].ToString() : null,
                            hip_name = reader["hip_name"] != DBNull.Value ? reader["hip_name"].ToString() : null,
                            access_mode = reader["access_mode"] != DBNull.Value ? reader["access_mode"].ToString() : null,
                            date_range_from = dateRangeFrom,
                            date_range_to = dateRangeTo,
                            data_erase_at = dataEraseAt,
                            consent_responded_on = consentRespondedOn,
                            granted_hi_types = grantedHiTypes
                        };

                        parent.consents.Add(child);
                    }

                    return parentConsents;
                }
            }
        }


        private DateTime? ParseDateTime(object dbValue)
        {
            if (dbValue == DBNull.Value || dbValue == null)
                return null;

            // Handle timestamp without time zone as DateTime
            if (DateTime.TryParse(dbValue.ToString(), out var dt))
            {
                return dt; // No timezone handling needed for DateTime
            }

            return null;
        }

        public async Task<int> InsertHIRequestData(HealthInformationRequestInsert obj)
        {
            var parameters = new List<NpgsqlParameter>
    {
        new NpgsqlParameter("@p_request_id", NpgsqlDbType.Varchar) { Value = obj.request_id },
        new NpgsqlParameter("@p_timestamp", NpgsqlDbType.Varchar) { Value = obj.timestamp },
        new NpgsqlParameter("@p_consent_id", NpgsqlDbType.Varchar) { Value = obj.consent_id },
        new NpgsqlParameter("@p_expiry", NpgsqlDbType.Timestamp) { Value =Utilities.ConvertToLocalTime( obj.expiry_date) },
        new NpgsqlParameter("@p_from", NpgsqlDbType.Timestamp) { Value =Utilities.ConvertToLocalTime( obj.from_date) },
        new NpgsqlParameter("@p_to", NpgsqlDbType.Timestamp) { Value =Utilities.ConvertToLocalTime( obj.to_date) },
        new NpgsqlParameter("@p_source_id", NpgsqlDbType.Integer) { Value = obj.source_id },
        new NpgsqlParameter("@p_receiver_public_key", NpgsqlDbType.Varchar) { Value = obj.receiver_public_key },
        new NpgsqlParameter("@p_receiver_private_key", NpgsqlDbType.Varchar) { Value = obj.receiver_private_key },
        new NpgsqlParameter("@p_receiver_nonce", NpgsqlDbType.Varchar) { Value = obj.receiver_nonce }
    };

            var result = await dbContext.Database.ExecuteSqlRawAsync(@"CALL public.sp_hiu_hi_request_insert(
            @p_request_id::character varying, 
            @p_timestamp::character varying, 
            @p_consent_id::character varying, 
            @p_expiry::timestamp without time zone, 
            @p_from::timestamp without time zone, 
            @p_to::timestamp without time zone, 
            @p_source_id::integer, 
            @p_receiver_public_key::character varying, 
            @p_receiver_private_key::character varying, 
            @p_receiver_nonce::character varying)", parameters.ToArray());

            return result;
        }

        public async Task<dynamic> HealthInformationRequest(HealthInformationCmRequest obj)
        {
            string accessToken = await accessTokenService.GetAccessToken();
            HttpClientHandler handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
            };
            var client = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Post, baseUrl + "health-information/cm/request");
            request.Headers.Add("X-CM-ID", XCMID);
            request.Headers.Add("Authorization", "Bearer " + accessToken);
            var serializeContent = System.Text.Json.JsonSerializer.Serialize(obj);
            request.Content = new StringContent(serializeContent, Encoding.UTF8, "application/json");
            var response = await client.SendAsync(request);
            return response;
        }
    }
}

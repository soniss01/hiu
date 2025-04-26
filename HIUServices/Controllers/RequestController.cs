using HIUServices.DbContexts;
using HIUServices.Entities;
using HIUServices.Entities.Requests;
using HIUServices.Entities.Requests.Consents;
using HIUServices.Entities.Requests.Data;
using HIUServices.Entities.Requests.Patients;
using HIUServices.Helpers;
using HIUServices.Repostitories.Requests.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace HIUServices.Controllers
{
    [ApiController]
    [Tags("Request APIs")]
    [Route("api/request/v1")]
    public class RequestController : ControllerBase
    {
        private readonly IRequestService requestService;
        private readonly IConfiguration configuration;
        private readonly ILogger<RequestController> logger;
        private readonly string HIU;
        private readonly string HIP;
        private readonly string DataPushUrl;

        public RequestController(IRequestService requestService, IConfiguration configuration, ILogger<RequestController> _logger)
        {
            this.requestService = requestService;
            this.configuration = configuration;
            logger = _logger;
            HIU = configuration.GetValue<string>("AbdmConfiguraton:hiu");
            HIP = configuration.GetValue<string>("AbdmConfiguraton:hip");
            DataPushUrl = configuration.GetValue<string>("AbdmConfiguraton:dataPushUrl");
            logger.LogInformation("=================================================");
            logger.LogInformation("Request API Started at:" + DateTime.Now);
        }


        [HttpPost("patients/find")]
        public async Task<Object> PatientsFind([FromBody] PatientRequestDto reqObj)
        {
            try
            {
                logger.LogInformation("patients/find call");

                if (ModelState.IsValid)
                {
                    var result = String.Empty;
                    Guid requestId = Guid.NewGuid();
                    logger.LogInformation("guid request id" + requestId);

                    var patient = new PatientFind.Query.Patient()
                    {
                        id = reqObj.patient_id
                    };
                    var requester = new PatientFind.Query.Requester()
                    {
                        id = HIU,
                        type = "HIU"
                    };
                    var query = new PatientFind.Query()
                    {
                        patient = patient,
                        requester = requester
                    };
                    var patientFind = new PatientFind()
                    {
                        requestId = Convert.ToString(requestId),
                        timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffffff"),
                        query = query
                    };

                    var insertPatientFind = new PatientFindInsert()
                    {
                        anm_id = reqObj.anm_id,
                        anm_name = reqObj.anm_name,
                        state_code = reqObj.state_code,
                        patient_id = reqObj.patient_id,
                        request_id = Convert.ToString(requestId),
                        source_id = reqObj.source_id,
                        abha_no = reqObj.abha_no,
                        dob=reqObj.dob
                    };

                    logger.LogInformation("request id" + Convert.ToString(requestId));
                    await requestService.InsertPatientFind(insertPatientFind);

                    var response = await requestService.PatientsFind(patientFind);

                    if (response.IsSuccessStatusCode)
                    {
                        logger.LogInformation(Convert.ToString(DateTime.Now));
                        await Task.Delay(1000 * 5);

                        DataTable dataTable = await requestService.GetPatientByRequestId(requestId.ToString());
                        var data = Utilities.DataTableToObject(dataTable);

                        if (dataTable != null && dataTable.Rows.Count > 0)
                        {
                            return Ok(CustomApiResponse.GetResponse(StatusCodes.Status200OK, "data found", data));
                        }
                        else
                        {
                            return Ok(CustomApiResponse.GetResponse(StatusCodes.Status404NotFound, "data not found", data));
                        }
                    }
                    var infoResult = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, CustomApiResponse.GetResponse((int)response.StatusCode, "patient not found", infoResult));
                }
                else
                {
                    EntityValidation[] validations = ModelState
                                      .Where(e => e.Value.Errors.Count > 0)
                                      .Select(e => new EntityValidation
                                      {
                                          name = e.Key,
                                          description = e.Value.Errors.First().ErrorMessage
                                      }).ToArray();

                    logger.LogInformation("patients/find call", validations);
                    return BadRequest(CustomApiResponse.GetResponse(StatusCodes.Status400BadRequest, "bad request", null, validations));
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Error:" + ex.Message + " " + ex.InnerException + "" + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, CustomApiResponse.GetResponse(StatusCodes.Status500InternalServerError, ex.Message + " " + ex.InnerException + "" + ex.StackTrace));
            }
        }

        [HttpPost("consent-requests/init")]
        public async Task<Object> ConsentRequests([FromBody] ConsentRequestDto consentRequestDto)
        {
            try
            {

                logger.LogInformation("consent-requests/init");
                Guid requestId = Guid.NewGuid();
                if (ModelState.IsValid)
                {
                    var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                    var consentPurpose = new ConsentRequest.Consents.ConsentPurpose()
                    {
                        text = consentRequestDto.purpose_text,
                        code = consentRequestDto.purpose_code
                    };
                    var consentPatient = new ConsentRequest.Consents.ConsentPatient()
                    {
                        id = consentRequestDto.patient_id
                    };
                    var consentHiu = new ConsentRequest.Consents.ConsentHiu()
                    {
                        id = HIU
                    };
                    var requesterIdentifier = new ConsentRequest.Consents.ConsentRequester.Identifier()
                    {
                        type = "REGNO",
                        value = Convert.ToString(consentRequestDto.anm_id),
                        system = "https://www.mciindia.org"
                    };
                    var consentRequester = new ConsentRequest.Consents.ConsentRequester()
                    {
                        name = consentRequestDto.anm_name,
                        identifier = requesterIdentifier
                    };
                    var consentPermissionDateRange = new ConsentRequest.Consents.ConsentPermission.ConsentPermissionDateRange()
                    {
                        from = consentRequestDto.permission_date_from.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        to = consentRequestDto.permission_date_to.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                    };
                    var consentPermissionFrequency = new ConsentRequest.Consents.ConsentPermission.ConsentPermissionFrequency()
                    {
                        unit = "HOUR",
                        value = 1,
                        repeats = 0
                    };
                    var consentPermission = new ConsentRequest.Consents.ConsentPermission()
                    {
                        accessMode = "VIEW",
                        dateRange = consentPermissionDateRange,
                        dataEraseAt = consentRequestDto.data_erase_at.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        frequency = consentPermissionFrequency
                    };
                    var consents = new ConsentRequest.Consents()
                    {
                        purpose = consentPurpose,
                        patient = consentPatient,
                        hip = null,
                        careContexts=null,
                        hiu = consentHiu,
                        requester = consentRequester,
                        hiTypes = consentRequestDto.hi_types,
                        permission = consentPermission
                    };

                    var consentRequest = new ConsentRequest()
                    {
                        requestId = requestId.ToString(),
                        timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        consent = consents
                    };
                    var insertConsentRequest = new ConsentRequestsInsert()
                    {
                        anm_id = consentRequestDto.anm_id ?? 0,
                        anm_name = consentRequestDto.anm_name ?? null,
                        state_code = consentRequestDto.state_code ?? 0,
                        request_id = Convert.ToString(requestId),
                        patient_id = consentRequestDto.patient_id,
                        patient_name = consentRequestDto.patient_name,
                        hiu = HIU,
                        purpose_code = consentRequestDto.purpose_code,
                        requester_id = consentRequestDto.anm_id ?? 0,
                        hi_types = String.Join(",", consentRequestDto.hi_types),
                        permission_date_from = Utilities.ConvertToLocalTime(consentRequestDto.permission_date_from),
                        permission_date_to = Utilities.ConvertToLocalTime(consentRequestDto.permission_date_to),
                        data_erase_at = Utilities.ConvertToLocalTime(consentRequestDto.data_erase_at),
                        source_id = consentRequestDto.source_id ?? 0,
                        abha_no = consentRequestDto.abha_no,
                        dob = consentRequestDto.dob
                    };

                    var result = await requestService.InsertConsentRequest(insertConsentRequest);

                    if (result == 2)
                    {
                        return BadRequest(CustomApiResponse.GetResponse(StatusCodes.Status400BadRequest, "Consent request already exists for the same date range"));
                    }
                    var response = await requestService.ConsentRequests(consentRequest);
                    var serializeContent = JsonSerializer.Serialize(consentRequest);
                    logger.LogInformation("consent Request:" + serializeContent);
                    var consentInfo = new ConsentRequestInfo();
                    if (response.IsSuccessStatusCode)
                    {
                        await Task.Delay(1000 * 5);

                        DataTable dataTable = await requestService.GetConsentByRequestId(requestId.ToString());
                        var data = Utilities.DataTableToObject(dataTable);

                        if (dataTable != null && dataTable.Rows.Count > 0)
                        {
                            return Ok(CustomApiResponse.GetResponse(StatusCodes.Status200OK, "data found", data));
                        }
                        else
                        {
                            return Ok(CustomApiResponse.GetResponse(StatusCodes.Status404NotFound, "data not found", data));
                        }
                    }
                    var infoResult = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, CustomApiResponse.GetResponse((int)response.StatusCode, "data not found", infoResult));
                }
                else
                {
                    EntityValidation[] validations = ModelState
                                      .Where(e => e.Value.Errors.Count > 0)
                                      .Select(e => new EntityValidation
                                      {
                                          name = e.Key,
                                          description = e.Value.Errors.First().ErrorMessage
                                      }).ToArray();

                    logger.LogInformation("consent-requests/init", validations);
                    return BadRequest(CustomApiResponse.GetResponse(StatusCodes.Status400BadRequest, "bad request", null, validations));
                }

            }
            catch (Exception ex)
            {
                logger.LogError("Error:" + ex.Message + " " + ex.InnerException + "" + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, CustomApiResponse.GetResponse(StatusCodes.Status500InternalServerError, ex.Message + " " + ex.InnerException + "" + ex.StackTrace));
            }
        }

        [HttpPost("health-information/request")]
        public async Task<Object> HealthInformationRequest([FromBody] HealthInformationCmRequestDto requestObj)
        {
            try
            {
                logger.LogInformation("health-information/request");
                var result = String.Empty;
                Guid requestId = Guid.NewGuid();
                var receiverKeyPair = DHKeyExchangeCrypto.GenerateKey();
                var publicKey = DHKeyExchangeCrypto.GetPublicKey(receiverKeyPair);
                var privateKey = DHKeyExchangeCrypto.GetPrivateKey(receiverKeyPair);
                var nonce = DHKeyExchangeCrypto.GenerateRandomKey();

                if (ModelState.IsValid)
                {

                    var hidData = await requestService.GetHIDataByConsentId(requestObj.consent_id);

                    if (hidData.Length != 0)
                    {
                        return Ok(CustomApiResponse.GetResponse(StatusCodes.Status200OK, "data found", hidData));
                    }
                    else
                    {
                        string timeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

                        var dateRangeObj = new DateRange()
                        {
                            from = DateTime.SpecifyKind(requestObj.range_from, DateTimeKind.Utc).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                            to = DateTime.SpecifyKind(requestObj.range_to, DateTimeKind.Utc).ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                        };

                        var hiRequestConsentObj = new HiRequestConsent()
                        {
                            id = requestObj.consent_id
                        };

                        var dhPublicKeyObj = new DhPublicKey()
                        {
                            expiry = DateTime.SpecifyKind(requestObj.expiry, DateTimeKind.Utc).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                            parameters = "Curve25519/32byte random key",
                            keyValue = publicKey,
                        };

                        var keyMaterialObj = new KeyMaterial()
                        {
                            cryptoAlg = "ECDH",
                            curve = "Curve25519",
                            dhPublicKey = dhPublicKeyObj,
                            nonce = nonce
                        };

                        var hiRequestObj = new HiRequest()
                        {
                            consent = hiRequestConsentObj,
                            dateRange = dateRangeObj,
                            dataPushUrl = DataPushUrl,
                            keyMaterial = keyMaterialObj
                        };

                        var hiRequests = new HealthInformationCmRequest()
                        {
                            requestId = requestId.ToString(),
                            timestamp = timeStamp,
                            hiRequest = hiRequestObj
                        };

                        var healthInformationRequestInsert = new HealthInformationRequestInsert()
                        {
                            request_id = requestId.ToString(),
                            timestamp = timeStamp,
                            consent_id = requestObj.consent_id,
                            expiry_date = Utilities.ConvertToLocalTime(requestObj.expiry),
                            from_date = Utilities.ConvertToLocalTime(requestObj.range_from),
                            to_date = Utilities.ConvertToLocalTime(requestObj.range_to),
                            receiver_public_key = publicKey,
                            receiver_private_key = privateKey,
                            receiver_nonce = nonce,
                            source_id = requestObj.source_id,
                        };


                        await requestService.InsertHIRequestData(healthInformationRequestInsert);

                        var response = await requestService.HealthInformationRequest(hiRequests);
                        var serializeContent = JsonSerializer.Serialize(hiRequests);
                        logger.LogInformation("consent Request:" + serializeContent);
                        var hiDataInfo = new HI_Data_Info();
                        if (response.IsSuccessStatusCode)
                        {
                            await Task.Delay(1000 * 15);
                            var data = await requestService.GetHIDataByConsentId(requestObj.consent_id);

                            if (data != null)
                            {
                                return Ok(CustomApiResponse.GetResponse(StatusCodes.Status200OK, "data found", data));
                            }
                            else
                            {
                                return Ok(CustomApiResponse.GetResponse(StatusCodes.Status404NotFound, "data not found", data));
                            }
                        }
                        var infoResult = await response.Content.ReadAsStringAsync();
                        return StatusCode((int)response.StatusCode, CustomApiResponse.GetResponse((int)response.StatusCode, "data not found", infoResult));
                    }
                }
                else
                {
                    EntityValidation[] validations = ModelState
                                      .Where(e => e.Value.Errors.Count > 0)
                                      .Select(e => new EntityValidation
                                      {
                                          name = e.Key,
                                          description = e.Value.Errors.First().ErrorMessage
                                      }).ToArray();

                    logger.LogInformation("consent-requests/init", validations);
                    return BadRequest(CustomApiResponse.GetResponse(StatusCodes.Status400BadRequest, "bad request", null, validations));
                }

            }
            catch (Exception ex)
            {
                logger.LogError("Error:" + ex.Message + " " + ex.InnerException + "" + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, CustomApiResponse.GetResponse(StatusCodes.Status500InternalServerError, ex.Message + " " + ex.InnerException + "" + ex.StackTrace));
            }
        }

        [HttpPost("consent-requests/data")]
        public async Task<IActionResult> ConsentRequestsData([FromBody] ConsentRequestsDataDto consentRequestDataDto)
        {
            try
            {
                logger.LogInformation("consent-requests/data");
                if (ModelState.IsValid)
                {
                    var data = await requestService.ConsentRequestsData(consentRequestDataDto);
                    //var data = Utilities.DataTableToObject(dataTable);

                    if (data != null)
                    {
                        return Ok(CustomApiResponse.GetResponse(StatusCodes.Status200OK, "data found", data));
                    }
                    else
                    {
                        return Ok(CustomApiResponse.GetResponse(StatusCodes.Status404NotFound, "data not found", data));
                    }
                }
                else
                {
                    EntityValidation[] validations = ModelState
                                      .Where(e => e.Value.Errors.Count > 0)
                                      .Select(e => new EntityValidation
                                      {
                                          name = e.Key,
                                          description = e.Value.Errors.First().ErrorMessage
                                      }).ToArray();

                    logger.LogInformation("consent-requests/data", validations);
                    return BadRequest(CustomApiResponse.GetResponse(StatusCodes.Status400BadRequest, "bad request", null, validations));
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Error:" + ex.Message + " " + ex.InnerException + "" + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, CustomApiResponse.GetResponse(StatusCodes.Status500InternalServerError, ex.Message + " " + ex.InnerException + "" + ex.StackTrace));
            }
        }

        [HttpPost("health-information/data")]
        public async Task<IActionResult> HealthInformationData([FromBody] HealthInformationDataReq obj)
        {
            try
            {
                logger.LogInformation("health-information/data");
                if (ModelState.IsValid)
                {
                    var data = await requestService.GetHIDataByConsentId(obj.consent_id);
                    if (data != null)
                    {
                        return Ok(CustomApiResponse.GetResponse(StatusCodes.Status200OK, "data found", data));
                    }
                    else
                    {
                        return Ok(CustomApiResponse.GetResponse(StatusCodes.Status404NotFound, "data not found", data));
                    }
                }
                else
                {
                    EntityValidation[] validations = ModelState
                                      .Where(e => e.Value.Errors.Count > 0)
                                      .Select(e => new EntityValidation
                                      {
                                          name = e.Key,
                                          description = e.Value.Errors.First().ErrorMessage
                                      }).ToArray();

                    logger.LogInformation("health-information/data", validations);
                    return BadRequest(CustomApiResponse.GetResponse(StatusCodes.Status400BadRequest, "bad request", null, validations));
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Error:" + ex.Message + " " + ex.InnerException + "" + ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, CustomApiResponse.GetResponse(StatusCodes.Status500InternalServerError, ex.Message + " " + ex.InnerException + "" + ex.StackTrace));
            }
        }
    }

}

using HIUServices.Entities.Callbacks;
using HIUServices.Helpers;
using HIUServices.Repostitories.Callbacks.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace HIUServices.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Tags("Callback APIs")]
    [Route("api/callback")]
    public class CallbackController : ControllerBase
    {
        private readonly ICallbackService callbackService;
        private readonly ILogger<CallbackController> logger;
        //this call back
        public CallbackController(ICallbackService callbackService, ILogger<CallbackController> _logger)
        {
            this.callbackService = callbackService;
            logger = _logger;
            logger.LogInformation("=================================================");
            logger.LogInformation("Request API Started at:" + DateTime.Now);
        }

        [HttpPost("v0.5/patients/on-find")]
        public async Task<IActionResult> PatientOnFind()
        {
            try
            {
                logger.LogInformation("=================================================");
                logger.LogInformation("patients/on-find call at:" + DateTime.Now);

                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    string responseBody = await reader.ReadToEndAsync();

                    Console.WriteLine("Received callback response: " + responseBody);

                    logger.LogInformation(responseBody);
                    PatientFindResponse patientData = System.Text.Json.JsonSerializer.Deserialize<PatientFindResponse>(responseBody);
                    if (patientData.patient != null)
                    {
                        logger.LogError(patientData.patient.id + "------" + patientData.patient.name);
                    }
                    if (patientData.error != null)
                    {
                        logger.LogError(patientData.error.code + "------" + patientData.error.code);
                    }

                    logger.LogInformation("patient info:" + responseBody);


                    var response = await callbackService.PatientFind(patientData);
                    logger.LogInformation("response info:" + response);
                }
                return Ok("Callback response received successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message, ex.StackTrace);
                Console.WriteLine("Error handling callback response: " + ex.Message);
                return StatusCode(500, ex.Message + " " + ex.InnerException + "" + ex.StackTrace);
            }
        }

        [HttpPost("v0.5/consent-requests/on-init")]
        public async Task<IActionResult> ConsentRequestsOnInit()
        {
            try
            {
                logger.LogInformation("=================================================");
                logger.LogInformation("consent-requests/on-init call at:" + DateTime.Now);

                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false))
                {
                    string responseBody = await reader.ReadToEndAsync();

                    // Process the callback response here
                    // For example, log it or deserialize it into an object
                    Console.WriteLine("Received callback response: " + responseBody);

                    logger.LogInformation(responseBody);
                    ConsentRequestsResponse consentData = System.Text.Json.JsonSerializer.Deserialize<ConsentRequestsResponse>(responseBody);
                    if (consentData.consentRequest != null)
                    {
                        logger.LogError(consentData.consentRequest.id);
                    }
                    if (consentData.error != null)
                    {
                        logger.LogError(consentData.error);
                    }

                    var response = await callbackService.ConsentRequest(consentData);
                    logger.LogInformation("response info:" + response);
                }
                return Ok("Callback response received successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message, ex.StackTrace);
                Console.WriteLine("Error handling callback response: " + ex.Message);
                return StatusCode(500, ex.Message + " " + ex.InnerException + "" + ex.StackTrace);
            }
        }

        [HttpPost("v0.5/consents/hiu/notify")]
        public async Task<IActionResult> ConsentsHiuNotify()
        {
            try
            {
                logger.LogInformation("=================================================");
                logger.LogInformation("consents/hiu/notify call at: " + DateTime.Now);

                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false))
                {
                    string responseBody = await reader.ReadToEndAsync();

                    // Log the received callback response
                    logger.LogInformation("Received JSON: " + responseBody);

                    // Deserialize the response body into the model class
                    ConsentsHiuNotifyResponse consentData = JsonSerializer.Deserialize<ConsentsHiuNotifyResponse>(responseBody);

                    // Serialize the consentData object to a JSON string
                    string consentDataJson = JsonSerializer.Serialize(consentData, new JsonSerializerOptions { WriteIndented = true });

                    // Log the serialized JSON string of the object
                    logger.LogInformation("Deserialized and Serialized JSON: " + consentDataJson);

                    // Call the service method to handle database insertion
                    var result = await callbackService.ConsentsHiuNotify(consentData);
                    logger.LogInformation("Database insertion result: " + result);
                }
                return Ok("Callback response received successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message, ex.StackTrace);
                return StatusCode(500, ex.Message + " " + ex.StackTrace);
            }
        }

        //[HttpPost("v0.5/consents/on-fetch")]
        //public async Task<IActionResult> ConsentDetails()
        //{
        //    try
        //    {
        //        logger.LogInformation("=================================================");
        //        logger.LogInformation("consents/on-fetch call at: " + DateTime.Now);

        //        using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false))
        //        {
        //            string responseBody = await reader.ReadToEndAsync();

        //            // Log the received callback response
        //            logger.LogInformation("Received JSON: " + responseBody);

        //            // Deserialize the response body into the model class
        //            ConsentsHiuNotifyResp consentData = JsonSerializer.Deserialize<ConsentsHiuNotifyResp>(responseBody);

        //            // Extract the care contexts as a comma-separated string
        //            string careContexts = string.Join(",", consentData.consent.consentDetail.careContexts.Select(cc => cc.careContextReference));

        //            // Create an instance of the model class
        //            var updateModel = new HiuConsentDetailUpdate
        //            {
        //                consent_artefacts_id = consentData.consent.consentDetail.consentId,
        //                hip_id = consentData.consent.consentDetail.hip.id,
        //                hip_name = consentData.consent.consentDetail.hip.name,
        //                access_mode = consentData.consent.consentDetail.permission.accessMode,
        //                date_range_from = consentData.consent.consentDetail.permission.dateRange.from,
        //                date_range_to = consentData.consent.consentDetail.permission.dateRange.to,
        //                data_erase_at = consentData.consent.consentDetail.permission.dataEraseAt,
        //                care_contexts = careContexts
        //            };

        //            // Call the procedure function
        //            var result = await callbackService.ConsentsDetailsUpdate(updateModel);

        //            logger.LogInformation("Database update result: " + result);
        //        }
        //        return Ok("Callback response received successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, ex.Message, ex.StackTrace);
        //        return StatusCode(500, ex.Message + " " + ex.StackTrace);
        //    }
        //}
        [HttpPost("v0.5/consents/on-fetch")]
        public async Task<IActionResult> ConsentDetails()
        {
            try
            {
                logger.LogInformation("=================================================");
                logger.LogInformation("consents/on-fetch call at: " + DateTime.Now);

                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false))
                {
                    string responseBody = await reader.ReadToEndAsync();

                    // Log the received callback response
                    logger.LogInformation("Received JSON: " + responseBody);

                    // Deserialize the response body into the model class
                    ConsentsHiuNotifyResp consentData = JsonSerializer.Deserialize<ConsentsHiuNotifyResp>(responseBody);

                    // Extract the care contexts as a comma-separated string
                    string careContexts = string.Join(",", consentData.consent.consentDetail.careContexts.Select(cc => cc.careContextReference));

                    // Extract hiTypes as a comma-separated string
                    string hiTypes = string.Join(",", consentData.consent.consentDetail.hiTypes);

                    // Create an instance of the model class
                    var updateModel = new HiuConsentDetailUpdate
                    {
                        consent_artefacts_id = consentData.consent.consentDetail.consentId,
                        hip_id = consentData.consent.consentDetail.hip.id,
                        hip_name = consentData.consent.consentDetail.hip.name,
                        access_mode = consentData.consent.consentDetail.permission.accessMode,
                        date_range_from = Utilities.ConvertToLocalTime(consentData.consent.consentDetail.permission.dateRange.from),
                        date_range_to = Utilities.ConvertToLocalTime(consentData.consent.consentDetail.permission.dateRange.to),
                        data_erase_at = Utilities.ConvertToLocalTime(consentData.consent.consentDetail.permission.dataEraseAt),
                        care_contexts = careContexts,
                        hi_types = hiTypes
                    };

                    // Call the procedure function
                    var result = await callbackService.ConsentsDetailsUpdate(updateModel);

                    logger.LogInformation("Database update result: " + result);
                }
                return Ok("Callback response received successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message, ex.StackTrace);
                return StatusCode(500, ex.Message + " " + ex.StackTrace);
            }
        }


        [HttpPost("v0.5/health-information/hiu/on-request")]
        public async Task<IActionResult> HealthInformationHiuOnRequest()
        {
            try
            {
                logger.LogInformation("=================================================");
                logger.LogInformation("health-information/hiu/on-request call at: " + DateTime.Now);

                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false))
                {
                    string responseBody = await reader.ReadToEndAsync();

                    // Log the received callback response
                    logger.LogInformation("Received JSON: " + responseBody);

                    // Deserialize the response body into the model class
                    HealthInformationHiuOnRequest consentData = JsonSerializer.Deserialize<HealthInformationHiuOnRequest>(responseBody);

                    // Serialize the consentData object to a JSON string
                    string consentDataJson = JsonSerializer.Serialize(consentData, new JsonSerializerOptions { WriteIndented = true });

                    // Log the serialized JSON string of the object
                    logger.LogInformation("Deserialized and Serialized JSON: " + consentDataJson);

                    // Call the service method to handle database insertion
                    var result = await callbackService.HealthInformationHiuOnResponse(consentData);
                    logger.LogInformation("Database insertion result: " + result);
                }
                return Ok("Callback response received successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message, ex.StackTrace);
                return StatusCode(500, ex.Message + " " + ex.StackTrace);
            }
        }

        [HttpPost("v0.5/health-information/data/transfer")]
        public async Task<IActionResult> HealthInformationDataTransfer()
        {
            try
            {
                logger.LogInformation("=================================================");
                logger.LogInformation("health-information/data/transfer call at: " + DateTime.Now);

                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false))
                {
                    string responseBody = await reader.ReadToEndAsync();

                    // Log the received callback response
                    logger.LogInformation("Received JSON: " + responseBody);

                    // Deserialize the response body into the model class
                    HealthInformationData data = JsonSerializer.Deserialize<HealthInformationData>(responseBody);

                    // Serialize the consentData object to a JSON string
                    string serializedJson = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

                    // Log the serialized JSON string of the object
                    logger.LogInformation("Deserialized and Serialized JSON: " + serializedJson);
                   

                    foreach (var entry in data.entries)
                    {
                        //DateTime dateTime = DateTime.Parse(data.keyMaterial.dhPublicKey.expiry, null, System.Globalization.DateTimeStyles.RoundtripKind);
                        //string formattedDateTime = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                        var hiData = new HIData()
                        {
                           //expiry=DateTime.SpecifyKind(DateTime.ParseExact(formattedDateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), DateTimeKind.Unspecified) ,
                           expiry= Utilities.ConvertToLocalTime(data.keyMaterial.dhPublicKey.expiry),
                           transactionId = data.transactionId,
                           publicKey= data.keyMaterial?.dhPublicKey?.keyValue,
                           nonce= data.keyMaterial?.nonce,
                           content=entry.content,
                           care_context=entry.careContextReference
                        };
                        var result = await callbackService.HealthInformationTransfer(hiData);
                        logger.LogInformation("Database insertion result: " + result);
                    }
                }
                return Ok("Callback response received successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message, ex.StackTrace);
                return StatusCode(500, ex.Message + " " + ex.StackTrace);
            }
        }
    }
}

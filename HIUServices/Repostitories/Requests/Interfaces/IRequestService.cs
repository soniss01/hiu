using HIUServices.Entities.Requests.Consents;
using HIUServices.Entities.Requests.Data;
using HIUServices.Entities.Requests.Patients;
using System.Data;
using System.Text.Json;

namespace HIUServices.Repostitories.Requests.Interfaces
{
    public interface IRequestService
    {
        Task<DataTable> GetPatientByRequestId(string requestId);

        Task<DataTable> GetConsentByRequestId(string requestId);

        Task<object[]> GetHIDataByConsentId(string consentId);

        Task<dynamic> PatientsFind(PatientFind obj);
        Task<dynamic> ConsentRequests(ConsentRequest obj);

        Task<int> InsertPatientFind(PatientFindInsert obj);

        Task<int> InsertConsentRequest(ConsentRequestsInsert consentRequests);
        Task<List<ParentConsent>> ConsentRequestsData(ConsentRequestsDataDto consentRequestDataDto);
        Task<int> InsertHIRequestData(HealthInformationRequestInsert obj);
        Task<dynamic> HealthInformationRequest(HealthInformationCmRequest obj);
    }
}

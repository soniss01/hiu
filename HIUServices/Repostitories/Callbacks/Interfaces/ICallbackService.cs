using HIUServices.Entities.Callbacks;

namespace HIUServices.Repostitories.Callbacks.Interfaces
{
    public interface ICallbackService
    {
        public Task<int> PatientFind(PatientFindResponse obj);

        public Task<int> ConsentRequest(ConsentRequestsResponse consentRequests);
        public Task<int> ConsentsHiuNotify(ConsentsHiuNotifyResponse response);
        public Task<int> ConsentsDetailsUpdate(HiuConsentDetailUpdate obj);
        public Task<int> HealthInformationHiuOnResponse(HealthInformationHiuOnRequest consentRequests);
        public Task<int> HealthInformationTransfer(HIData response);
    }
}

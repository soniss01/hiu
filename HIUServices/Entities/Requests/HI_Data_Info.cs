namespace HIUServices.Entities.Requests
{
    public class HI_Data_Info
    {
      
        public string RequestId { get; set; }
        public string ConsentId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string ReceiverPublicKey { get; set; }
        public string ReceiverPrivateKey { get; set; }
        public string ReceiverNonce { get; set; }
        public string TransactionId { get; set; }
        public string SessionStatus { get; set; }
        public string SenderPublicKey { get; set; }
        public string SenderNonce { get; set; }
        public string ResponseData { get; set; }
        
       
    }
}

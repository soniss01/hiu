namespace HIUServices.Entities.Requests.Patients
{
    public class PatientFind
    {
        public string requestId { get; set; }
        public string timestamp { get; set; }
        public Query query { get; set; }

        public class Query
        {
            public Patient patient { get; set; }
            public Requester requester { get; set; }

            public class Patient
            {
                public string id { get; set; }
            }
            public class Requester
            {
                public string type { get; set; }
                public string id { get; set; }
            }
        }
    }
}

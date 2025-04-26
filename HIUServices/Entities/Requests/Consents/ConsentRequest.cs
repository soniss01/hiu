namespace HIUServices.Entities.Requests.Consents
{
    public class ConsentRequest
    {
        public string requestId { get; set; }
        public string timestamp { get; set; }
        public Consents consent { get; set; }

        public class Consents
        {
            public ConsentPurpose purpose { get; set; }
            public ConsentPatient patient { get; set; }
            public ConsentHip? hip { get; set; }
            public ConsentHiu hiu { get; set; }
            public ConsentRequester requester { get; set; }
            public string[] hiTypes { get; set; }
            public ConsentPermission permission { get; set; }
            public CareContext[]? careContexts { get; set; }

            public class ConsentPurpose
            {
                public string text { get; set; }
                public string code { get; set; }
                public string refUri { get; set; }
            }

            public class ConsentPatient
            {
                public string id { get; set; }
            }

            public class ConsentHip
            {
                public string id { get; set; }
            }

            public class ConsentHiu
            {
                public string id { get; set; }
            }

            public class ConsentRequester
            {
                public string name { get; set; }
                public Identifier identifier { get; set; }

                public class Identifier
                {
                    public string type { get; set; }
                    public string value { get; set; }
                    public string system { get; set; }
                }
            }

            public class ConsentPermission
            {
                public string accessMode { get; set; }
                public ConsentPermissionDateRange dateRange { get; set; }
                public string dataEraseAt { get; set; }
                public ConsentPermissionFrequency frequency { get; set; }

                public class ConsentPermissionDateRange
                {
                    public string from { get; set; }
                    public string to { get; set; }
                }

                public class ConsentPermissionFrequency
                {
                    public string unit { get; set; }
                    public int value { get; set; }
                    public int repeats { get; set; }
                }
            }

            public class CareContext
            {
                public string patientReference { get; set; }
                public string careContextReference { get; set; }
            }
        }
    }
}

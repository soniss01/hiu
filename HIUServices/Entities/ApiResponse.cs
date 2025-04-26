namespace HIUServices.Entities
{
    public class ApiResponse
    {
        public int status_code { get; set; }
        public string message { get; set; }
        public object data { get; set; }
        public EntityValidation[] errors { get; set; }
    }
}

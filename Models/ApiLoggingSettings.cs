namespace RealEstateApi.Models
{
    public class ApiLoggingSettings
    {
        public bool Enable { get; set; }
        public int SlowApiThresholdMs { get; set; }
    }
}

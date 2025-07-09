using System.ComponentModel.DataAnnotations;

namespace RealEstateApi.Models
{
    public class AppConfigM
    {
        [Key]
        public int id { get; set; }
        public string? key { get; set; } = string.Empty;
        public string? value { get; set; } = string.Empty;
    }

    public class UpdateAppConfigDto
    {
        public bool value { get; set; }
    }
}

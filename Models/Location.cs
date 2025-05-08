using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RealEstateApi.Models
{
    public class Location
    {
    }

    public class Province
    {
        [Key]
        public int provinceId { get; set; }
        public string name { get; set; } = string.Empty;

        //public List<District>? districts { get; set; }
    }

    public class District
    {
        [Key]
        public int districtId { get; set; }
        public string name { get; set; } = string.Empty;
        public int provinceId { get; set; }

        //[ForeignKey("provinceId")]
        //public Province? province { get; set; }
        //public List<Ward>? wards { get; set; }
    }

    public class Ward
    {
        [Key]
        public int wardId { get; set; }
        public string name { get; set; } = string.Empty;
        public int districtId { get; set; }

        //[ForeignKey("districtId")]
        //public District? district { get; set; }
    }
}

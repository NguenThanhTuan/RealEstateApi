using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateApi.Models
{
    public class RealEstateImages
    {
        [Key]
        public int imageId { get; set; }
        [Required]
        public string imageUrl { get; set; } = string.Empty; // Đường dẫn đến ảnh
        public int realEstateId { get; set; } // Khóa ngoại đến bảng BĐS
        [ForeignKey("realEstateId")]
        public RealEstates? realEstate { get; set; } // Tham chiếu đến BĐS tương ứng
    }
}

using System.ComponentModel.DataAnnotations;

namespace RealEstateApi.Models
{
    public class RealEstates
    {
        [Key]
        public int realEstateId { get; set; }
        public string name { get; set; } = string.Empty; // Tên BĐS
        public decimal price { get; set; } // Giá
        public string province { get; set; } = string.Empty; // Tỉnh/Thành phố
        public string district { get; set; } = string.Empty; // Quận/Huyện
        public string ward { get; set; } = string.Empty; // Phường/Xã
        public string street { get; set; } = string.Empty; // Đường
        public string category { get; set; } = string.Empty; // Loại BĐS
        public string status { get; set; } = string.Empty; // Trạng thái BĐS
        public float area { get; set; } // Diện tích
        public float? roadWidth { get; set; } // Chiều rộng
        public int? floors { get; set; } // Số tầng
        public int? bedrooms { get; set; } // Số phòng ngủ
        public int? bathrooms { get; set; } // Số phòng tắm
        public string? direction { get; set; } // Hướng
        public string? description { get; set; } // Mô tả
        public float length { get; set; } // Chiều dài
        public float width { get; set; } // Chiều rộng
        public DateTime postedDate { get; set; } = DateTime.UtcNow;
        public DateTime? updatedDate { get; set; }

        public List<RealEstateImages> images { get; set; } = new List<RealEstateImages>(); // Danh sách ảnh liên quan đến BĐS
    }
}

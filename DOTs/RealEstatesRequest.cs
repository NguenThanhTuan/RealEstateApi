using RealEstateApi.Models;
using System.ComponentModel.DataAnnotations;

namespace RealEstateApi.DOTs
{
    public class RealEstatesRequest
    {
    }

    public class SearchRealEstatesRequest
    {
        public string? search { get; set; }
        public int pageIndex { get; set; } = 1;
        public int pageSize { get; set; } = 10;
    }

    public class RealEstateCreateRequest {
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

        public IFormFileCollection? images { get; set; }
    }

    public class RealEstateCreateRequest2
    {
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
    }

    public class RealEstateUpdateInfoRequest
    {
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
        public DateTime postedDate { get; set; } // Ngày đăng
    }

    public class RealEstateUpdateImagesRequest
    {
        public List<int>? imageIdsToDelete { get; set; }
        public List<IFormFile>? newImages { get; set; }
    }

    public class RealEstateUpdateRequest
    {
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
        public DateTime postedDate { get; set; } // Ngày đăng
        public DateTime updatedDate { get; set; } = DateTime.UtcNow; // Ngày cập nhật

        public IFormFileCollection? newImages { get; set; }
        public List<int>? imageIdsToDelete { get; set; }
    }

    public class RealEstateSearchRequest
    {
        public string? search { get; set; } // Tìm kiếm chung
        public int pageIndex { get; set; } = 1;
        public int pageSize { get; set; } = 10;
        //public PagingRequest? paging { get; set; }
        public SearchFilter? filter { get; set; }
    }

    public class PagingRequest
    {
        public int pageIndex { get; set; } = 1;
        public int pageSize { get; set; } = 10;
    }

    public class SearchFilter
    {
        public decimal? minPrice { get; set; }
        public decimal? maxPrice { get; set; }
        public float? minArea { get; set; }
        public float? maxArea { get; set; }
        public string? province { get; set; }
        public string? district { get; set; }
        public string? ward { get; set; }
        public string? street { get; set; }
        public string? category { get; set; }
        public string? status { get; set; }
    }

    public class UpdateStatusRequest
    {
        public string status { get; set; } = string.Empty;
    }

    public class UserDTO
    {
        public int userId { get; set; }
        public string? fullName { get; set; } = string.Empty;
        public string phoneNumber { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public int role { get; set; }
        public string? address { get; set; } = string.Empty;
        public DateTime? createdAt { get; set; }
        public bool isActive { get; set; } = false;
        public string? deviceId { get; set; }
        //public string fcmToken { get; set; } = string.Empty; // Firebase Cloud Messaging token
        //public string platform { get; set; } = string.Empty; // e.g., "android" or "ios"
    }

    public class RealEstateNotificationDto
    {
        public int realEstateId { get; set; }
        public string name { get; set; } = string.Empty;
        public decimal price { get; set; }
        public string province { get; set; } = string.Empty;
        public string district { get; set; } = string.Empty;
        public string ward { get; set; } = string.Empty;
        public string street { get; set; } = string.Empty;
        public string category { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public double area { get; set; }
        public double? roadWidth { get; set; }
        public int? floors { get; set; }
        public int? bedrooms { get; set; }
        public int? bathrooms { get; set; }
        public string direction { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public double? length { get; set; }
        public double? width { get; set; }
        public DateTime postedDate { get; set; }
        public DateTime updatedDate { get; set; }
        public int? createdBy { get; set; }
        public DateTime? highlightedDate { get; set; }
        public List<ImageDto>? images { get; set; }

        public class ImageDto
        {
            public int imageId { get; set; }
            public string imageUrl { get; set; } = string.Empty;
        }

        public class NotificationDto
        {
            public int notificationId { get; set; }
            public string? title { get; set; } = string.Empty;
            public string? body { get; set; } = string.Empty;
            public string? imageUrl { get; set; } = string.Empty;
            public int type { get; set; }
            public DateTime? createdAt { get; set; }
            public string? data { get; set; } = string.Empty;
            public bool isRead { get; set; }
            public int realEstateId { get; set; }
        }
    }
}

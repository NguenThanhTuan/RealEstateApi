using System.ComponentModel.DataAnnotations;
using System.Xml;

namespace RealEstateApi.Models
{
    public class Notifications
    {
        [Key]
        public int notificationId { get; set; }
        [Required]
        public string? title { get; set; } = string.Empty;
        [Required]
        public string? body { get; set; } = string.Empty;
        public string? imageUrl { get; set; } = string.Empty;
        [Required]
        public int type { get; set; }
        public DateTime? createdAt { get; set; } = DateTime.UtcNow;
        public int realEstateId { get; set; }
        public string? data { get; set; } = string.Empty;
        public RealEstates? RealEstate { get; set; }
        public virtual ICollection<NotificationRead>? NotificationReads { get; set; }
    }

    public class NotificationData
    {
        public string? name { get; set; } = string.Empty;
        public string? street { get; set; } = string.Empty;
        public string? ward { get; set; } = string.Empty;
        public string? district { get; set; } = string.Empty;
        public string? province { get; set; } = string.Empty;
        public DateTime? postedDate { get; set; }
        public string? imageUrl { get; set; } = string.Empty;
        public int type { get; set; }
        public int realEstateId { get; set; }
    }

    public class MarkReadRequest
    {
        public bool isRead { get; set; }
    }
}

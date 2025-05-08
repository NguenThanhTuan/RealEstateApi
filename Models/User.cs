namespace RealEstateApi.Models
{
    public class User
    {
        public int userId { get; set; }
        public string? fullName { get; set; } = string.Empty;
        public string phoneNumber { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public int role { get; set; }
        public string? address { get; set; } = string.Empty;
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public bool isActive { get; set; } = false;
        public string? deviceId { get; set; }
        public string? fcmToken { get; set; } = string.Empty; // Firebase Cloud Messaging token
        public string? platform { get; set; } = string.Empty; // e.g., "android" or "ios"
        public virtual ICollection<NotificationRead>? NotificationReads { get; set; }
    }

    public class LoginRequest
    {
        public string phoneNumber { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string deviceId { get; set; } = string.Empty;
    }

    public class LogoutRequest
    {
        public string phoneNumber { get; set; } = string.Empty;
        public string deviceId { get; set; } = string.Empty;
    }

    public class RegisterDeviceRequest
    {
        public string fcmToken { get; set; } = string.Empty; // Firebase Cloud Messaging token
        public string platform { get; set; } = string.Empty; // e.g., "android" or "ios"
    }
}

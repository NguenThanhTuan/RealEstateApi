using FirebaseAdmin.Messaging;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RealEstateApi.Models
{
    public class NotificationRead
    {
        [Key]
        public int id { get; set; }

        public int notificationId { get; set; }

        public int userId { get; set; }

        public bool isRead { get; set; } = false;

        public DateTime? readAt { get; set; }


        public Notifications? Notifications { get; set; }
        public User? User { get; set; }
    }
}

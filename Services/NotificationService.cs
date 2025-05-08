using System;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using System.Xml;
using Google;
using Microsoft.AspNetCore.SignalR.Protocol;
using RealEstateApi.Data;
using RealEstateApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using RealEstateApi.DOTs;
using System.Text.Json;

public class NotificationService : INotificationService
{

    private readonly AppDbContext _context;
    private readonly FirebaseService _firebaseService;
    public NotificationService (AppDbContext context, FirebaseService firebaseService)
    {
        _context = context;
        _firebaseService = firebaseService;
    }

    public async Task CreateAndSendNotificationAsync(int type, RealEstateNotificationDto estate, int currentUserId)
    {
        try
        {
            // B1: Tạo thông báo mới
            var parts = new List<string>();
            // Format lại giá
            string priceText = "";

            if (estate.price > 0)
            {
                if (estate.price >= 1000000000)
                    priceText = $"{estate.price / 1000000000:#,0.#} tỷ";
                else if (estate.price >= 1000000)
                    priceText = $"{estate.price / 1000000:#,0.#} triệu";
                else
                    priceText = $"{estate.price:#,0} đồng";

                parts.Add(priceText);
            }

            //var title = $"{estate.name} {(type == 1 ? " vừa mới được thêm vào kho hàng" : " vừa mới được cập nhật thông tin")}";
            var title = type == 1
                ? $"{estate.name} vừa mới được thêm vào kho hàng"
                : $"{estate.name} vừa mới được cập nhật thông tin";

            var addressParts = new List<string?>
            {
                estate.street,
                estate.ward,
                estate.district,
                estate.province
            };

            //var address = $"{estate.street} - {estate.ward} - {estate.district} - {estate.province}";
            var address = string.Join(" - ", addressParts.Where(p => !string.IsNullOrWhiteSpace(p)));
            // Địa chỉ
            if (!string.IsNullOrWhiteSpace(address))
            {
                parts.Add(address);
            }
            // Diện tích
            if (estate.area > 0)
            {
                parts.Add($"{estate.area} m²");
            }

            // Ghép thông tin chính (giá | địa chỉ | diện tích)
            var mainInfo = string.Join(" | ", parts);

            // Thời gian
            var createdAt = type == 1 ? estate.postedDate.ToLocalTime() : estate.updatedDate.ToLocalTime();
            var timeInfo = createdAt.ToString("dd/MM/yyyy HH:mm");

            // Gộp lại body (dòng chính + xuống dòng thời gian)
            //var description = $"{estate.price} | {address} | {estate.area} m²";
            //var description = $"{mainInfo}\n{timeInfo}";
            var description = $"{mainInfo}";

            // Lấy ảnh đầu tiên
            var imageUrl = estate.images?.FirstOrDefault()?.imageUrl!;

            var data = new NotificationData
            {
                name = estate.name,
                street = estate.street,
                ward = estate.ward,
                district = estate.district,
                province = estate.province,
                postedDate = estate.postedDate,
                imageUrl = imageUrl,
                type = type,
                realEstateId = estate.realEstateId
            };
            // Chuyển object thành chuỗi JSON
            string dataJson = JsonSerializer.Serialize(data);

            // Tạo bản ghi Notification
            var notification = new Notifications
            {
                title = title,
                body = description,
                imageUrl = imageUrl,
                type = type,
                createdAt = type == 1 ? estate.postedDate : DateTime.UtcNow,
                realEstateId = estate.realEstateId,
                data = dataJson
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // B2: Lấy danh sách người dùng
            // Lấy danh sách user có FCM token
            //var users = await _context.Users.Where(u => !string.IsNullOrEmpty(u.fcmToken)).ToListAsync();
            var users = await _context.Users.ToListAsync();

            var notificationReads = new List<NotificationRead>();
            var fcmTokens = new List<string>();

            // 3. Tạo NotificationReads cho tất cả
            var readRecords = users.Select(u => new NotificationRead
            {
                userId = u.userId,
                notificationId = notification.notificationId,
                isRead = u.userId == currentUserId
            }).ToList();

            _context.NotificationReads.AddRange(readRecords);
            await _context.SaveChangesAsync();

            //foreach (var user in users)
            //{
            //    var isRead = user.userId == currentUserId;

            //    // Tạo NotificationRead
            //    notificationReads.Add(new NotificationRead
            //    {
            //        notificationId = notification.notificationId,
            //        userId = user.userId,
            //        isRead = isRead
            //    });

            //    // Nếu không phải user hiện tại thì mới gửi FCM
            //    if (!isRead)
            //    {
            //        var tokens = await _context.Users
            //            .Where(d => d.userId == user.userId && !string.IsNullOrEmpty(d.fcmToken))
            //            .Select(d => d.fcmToken!)
            //            .ToListAsync();

            //        fcmTokens.AddRange(tokens);
            //    }
            //}


            // Tạo NotificationReads
            //var reads = users.Select(u => new NotificationRead
            //{
            //    notificationId = notification.notificationId,
            //    userId = u.userId,
            //    isRead = false
            //}).ToList();

            //_context.NotificationReads.AddRange(notificationReads);
            //await _context.SaveChangesAsync();


            // 4. Gửi FCM cho các user khác (trừ current)
            var otherUserTokens = users
                .Where(u => u.userId != currentUserId && !string.IsNullOrEmpty(u.fcmToken))
                .Select(u => u.fcmToken!)
                .ToList();
            await _firebaseService.SendMulticastNotificationAsync(otherUserTokens, title, description, imageUrl, estate);


            //if (fcmTokens.Any())
            //{
            //    // Gửi FCM
            //    foreach (var user in users)
            //    {
            //        await _firebaseService.SendMessageAsync(new PushRequest
            //        {
            //            token = user.fcmToken!,
            //            title = title,
            //            body = description,
            //            imageUrl = imageUrl ?? ""
            //        });
            //    }
            //}
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating notification: {ex.Message}");
        }
    }
}

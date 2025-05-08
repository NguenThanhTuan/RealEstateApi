using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApi.Data;
using RealEstateApi.DOTs;
using RealEstateApi.Models;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static RealEstateApi.DOTs.RealEstateNotificationDto;

namespace RealEstateApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public NotificationController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        //[Authorize]
        //[HttpGet("{userId}")]
        //public async Task<IActionResult> GetUserNotifications(int userId)
        //{
        //    try
        //    {
        //        //var currentUserId = GetCurrentUserId(); // Viết hàm này như ở RealEstateController
        //        var notifications = await _context.NotificationReads
        //            .Include(nr => nr.Notifications)
        //            .Where(nr => nr.userId == userId)
        //            .OrderByDescending(nr => nr.Notifications!.createdAt)
        //            .Select(nr => new NotificationDto
        //            {
        //                notificationId = nr.notificationId,
        //                title = nr.Notifications!.title,
        //                body = nr.Notifications.body,
        //                imageUrl = nr.Notifications.imageUrl,
        //                type = nr.Notifications.type,
        //                createdAt = nr.Notifications.createdAt,
        //                data = nr.Notifications.data,
        //                isRead = nr.isRead,
        //                realEstateId = nr.Notifications.realEstateId
        //            })
        //            .ToListAsync();

        //        return Ok(ApiResult.Success<object>(notifications, "Danh sách thông báo", 200));
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ApiResult.Error<object>($"Đã xảy ra lỗi. Lỗi: {ex.Message}", 400));
        //    }
        //}

        [Authorize]
        [HttpGet("list")]
        public async Task<IActionResult> GetUserNotifications(int pageIndex = 1, int pageSize = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                var query = _context.NotificationReads
                    .Include(nr => nr.Notifications)
                    .Where(nr => nr.userId == userId)
                    .OrderByDescending(nr => nr.Notifications!.createdAt);

                var totalCount = await query.CountAsync();

                var notifications = await query
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .Select(nr => new NotificationDto
                    {
                        notificationId = nr.notificationId,
                        title = nr.Notifications!.title,
                        body = nr.Notifications.body,
                        imageUrl = nr.Notifications.imageUrl,
                        type = nr.Notifications.type,
                        createdAt = nr.Notifications.createdAt,
                        data = nr.Notifications.data,
                        isRead = nr.isRead,
                        realEstateId = nr.Notifications.realEstateId
                    })
                    .ToListAsync();

                var res = new
                {
                    success = true,
                    items = notifications,
                    pageIndex = pageIndex,
                    totalRecords = totalCount,
                    message = "Danh sách thông báo",
                    code = 200
                };

                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Error<object>($"Đã xảy ra lỗi. Lỗi: {ex.Message}", 400));
            }
        }

        [Authorize]
        [HttpGet("listAll")]
        public async Task<IActionResult> GetUserNotificationsAll()
        {
            try
            {
                var userId = GetCurrentUserId();
                var notifications = await _context.NotificationReads
                    .Include(nr => nr.Notifications)
                    .Where(nr => nr.userId == userId)
                    .OrderByDescending(nr => nr.Notifications!.createdAt)
                    .Select(nr => new NotificationDto
                    {
                        notificationId = nr.notificationId,
                        title = nr.Notifications!.title,
                        body = nr.Notifications.body,
                        imageUrl = nr.Notifications.imageUrl,
                        type = nr.Notifications.type,
                        createdAt = nr.Notifications.createdAt,
                        data = nr.Notifications.data,
                        isRead = nr.isRead,
                        realEstateId = nr.Notifications.realEstateId
                    })
                    .ToListAsync();

                return Ok(ApiResult.Success<object>(notifications, "Danh sách thông báo", 200));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Error<object>($"Đã xảy ra lỗi. Lỗi: {ex.Message}", 400));
            }
        }

        [Authorize]
        [HttpPost("mark-read/{notificationId}")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var notificationRead = await _context.NotificationReads
                    .FirstOrDefaultAsync(nr => nr.notificationId == notificationId && nr.userId == currentUserId);

                if (notificationRead == null)
                    return NotFound(ApiResult.Error<object>("Thông báo không tồn tại"));

                if (!notificationRead.isRead)
                {
                    notificationRead.isRead = true;
                    notificationRead.readAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return Ok(ApiResult.Success<object>("Đánh dấu thông báo đã đọc thành công"));
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(ApiResult.Error<object>("Đánh dấu thông báo đã đọc không thành công", 400));
            }
            catch (DbUpdateException)
            {
                return BadRequest(ApiResult.Error<object>("Đánh dấu thông báo đã đọc không thành công", 400));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Error<object>($"Đánh dấu thông báo đã đọc không thành công. Lỗi: {ex.Message}", 500));
            }
        }

        [Authorize]
        [HttpDelete("{notificationId}")]
        public async Task<IActionResult> DeleteNotification(int notificationId)
        {
            try
            {
                var notification = await _context.Notifications
                    .Include(n => n.NotificationReads)
                    .FirstOrDefaultAsync(n => n.notificationId == notificationId);

                if (notification == null) return NotFound(ApiResult.Error<object>("Thông báo không tồn tại"));

                if (notification.NotificationReads != null && notification.NotificationReads.Any())
                {
                    // Xóa tất cả bản ghi trong bảng NotificationReads
                    _context.NotificationReads.RemoveRange(notification.NotificationReads);
                }

                // Xóa bản ghi chính trong bảng Notifications
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();

                return Ok(ApiResult.Success<object>("Xoá thông báo thành công"));
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(ApiResult.Error<object>("Xóa thông báo không thành công", 400));
            }
            catch (DbUpdateException)
            {
                return BadRequest(ApiResult.Error<object>("Xóa thông báo không thành công", 400));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Error<object>($"Xóa thông báo không thành công. Lỗi: {ex.Message}", 400));
            }
        }

        //[Authorize]
        //[HttpGet("list/{userId}")]
        //public async Task<IActionResult> GetNotifications(int userId)
        //{
        //    try { 
        //        var notifications = await _context.Notifications
        //            .Where(n => n.userId == userId)
        //            .OrderByDescending(n => n.createdAt)
        //            .ToListAsync();
        //        if (notifications == null || notifications.Count == 0)
        //        {
        //            return NotFound(ApiResult.Error<object>("Không tìm thấy thông báo", 404));
        //        }
        //        return Ok(ApiResult.Success<object>(notifications, "Kết quả tìm kiếm", 200));
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ApiResult.Error<object>($"Không tìm thấy thông báo. Lỗi: {ex.Message}", 400));
        //    }
        //}

        //[Authorize]
        //[HttpGet]
        //public async Task<IActionResult> GetAllNotifications()
        //{
        //    try
        //    {
        //        var data = await _context.Notifications
        //        .OrderByDescending(r => r.createdAt)
        //        .Select(r => new
        //        {
        //            r.notificationId,
        //            r.title,
        //            r.body,
        //            //r.userId,
        //            //r.isRead,
        //            r.createdAt
        //        }).ToListAsync();

        //        return Ok(ApiResult.Success<object>(data, "Danh sách thông báo", 200));
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ApiResult.Error<object>($"Không thể lấy danh sách. Lỗi: {ex.Message}", 400));
        //    }
        //}

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : throw new UnauthorizedAccessException("User ID not found");
        }
    }
}

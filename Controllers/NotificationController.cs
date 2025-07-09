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

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("list")]
        public async Task<IActionResult> GetUserNotifications(int pageIndex = 1, int pageSize = 10)
        {
            // Lấy config IOS
            var showNews = _context.AppConfigs
                .FirstOrDefault(x => x.key == "NewsScreen")?.value;

            var show = showNews == "true";
            var isAuthenticated = HttpContext.User.Identity?.IsAuthenticated == true;

            // Kiểm tra User-Agent để xác định thiết bị Android
            var userAgent = Request.Headers["User-Agent"].ToString();
            var isAndroid = userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase);

            // Lấy config Android
            var showNewsAndr = _context.AppConfigs
                .FirstOrDefault(x => x.key == "NewsScreenAndr")?.value;

            var showAndr = showNewsAndr == "true";

            try
            {
                int? userId = null;

                if (isAndroid)
                {
                    if (!showAndr)
                    {
                        if (!isAuthenticated)
                            return Unauthorized(ApiResult.Error<object>("Không có quyền truy cập", 401));

                        if (isAuthenticated)
                            userId = GetCurrentUserId();
                    }
                    else
                    {
                        // Nếu là public, có token thì ưu tiên lấy userId để lọc trạng thái isRead
                        if (isAuthenticated)
                            userId = GetCurrentUserId();
                    }
                }
                else
                {
                    if (!show)
                    {
                        // Nếu không phải public thì bắt buộc phải có user
                        //if (!isAuthenticated && !isAndroid)
                        if (!isAuthenticated)
                            return Unauthorized(ApiResult.Error<object>("Không có quyền truy cập", 401));

                        if (isAuthenticated)
                            userId = GetCurrentUserId();
                    }
                    else
                    {
                        // Nếu là public, có token thì ưu tiên lấy userId để lọc trạng thái isRead
                        if (isAuthenticated)
                            userId = GetCurrentUserId();
                    }
                }

                IQueryable<NotificationRead> query;

                if (userId.HasValue && userId != null)
                {
                    // Trường hợp có user => lọc theo userId
                    query = _context.NotificationReads
                        .Include(nr => nr.Notifications)
                        .Where(nr => nr.userId == userId)
                        .OrderByDescending(nr => nr.Notifications!.createdAt);
                }
                else
                {
                    // Trường hợp công khai => lấy tất cả notifications
                    query = _context.Notifications
                        .OrderByDescending(n => n.createdAt)
                        .Select(n => new NotificationRead
                        {
                            notificationId = n.notificationId,
                            Notifications = n,
                            isRead = false
                        }).AsQueryable();
                }

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

                //var userId = GetCurrentUserId();
                //var query = _context.NotificationReads
                //    .Include(nr => nr.Notifications)
                //    .Where(nr => nr.userId == userId)
                //    .OrderByDescending(nr => nr.Notifications!.createdAt);

                //var totalCount = await query.CountAsync();

                //var notifications = await query
                //    .Skip((pageIndex - 1) * pageSize)
                //    .Take(pageSize)
                //    .Select(nr => new NotificationDto
                //    {
                //        notificationId = nr.notificationId,
                //        title = nr.Notifications!.title,
                //        body = nr.Notifications.body,
                //        imageUrl = nr.Notifications.imageUrl,
                //        type = nr.Notifications.type,
                //        createdAt = nr.Notifications.createdAt,
                //        data = nr.Notifications.data,
                //        isRead = nr.isRead,
                //        realEstateId = nr.Notifications.realEstateId
                //    })
                //    .ToListAsync();

                //var res = new
                //{
                //    success = true,
                //    items = notifications,
                //    pageIndex = pageIndex,
                //    totalRecords = totalCount,
                //    message = "Danh sách thông báo",
                //    code = 200
                //};

                //return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Error<object>($"Đã xảy ra lỗi. Lỗi: {ex.Message}", 400));
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("listAll")]
        public async Task<IActionResult> GetUserNotificationsAll()
        {
            var showNews = _context.AppConfigs
                .FirstOrDefault(x => x.key == "NewsScreen")?.value;

            var show = showNews == "true";
            var isAuthenticated = HttpContext.User.Identity?.IsAuthenticated == true;
            var userAgent = Request.Headers["User-Agent"].ToString();
            var isAndroid = userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase);

            // Lấy config Android
            var showNewsAndr = _context.AppConfigs
                .FirstOrDefault(x => x.key == "NewsScreenAndr")?.value;

            var showAndr = showNewsAndr == "true";

            int? userId = null;


            if (isAndroid)
            {
                if (!showAndr)
                {
                    if (!isAuthenticated)
                        return Unauthorized(ApiResult.Error<object>("Không có quyền truy cập", 401));

                    if (isAuthenticated)
                        userId = GetCurrentUserId();
                }
                else
                {
                    // Nếu là public, có token thì ưu tiên lấy userId để lọc trạng thái isRead
                    if (isAuthenticated)
                        userId = GetCurrentUserId();
                }
            }
            else
            {
                if (!show)
                {
                    // Nếu không phải public thì bắt buộc phải có user
                    if (!isAuthenticated)
                        //if (!isAuthenticated && !isAndroid)
                        return Unauthorized(ApiResult.Error<object>("Không có quyền truy cập", 401));

                    if (isAuthenticated)
                        userId = GetCurrentUserId();
                }
                else
                {
                    // Nếu là public, có token thì ưu tiên lấy userId để lọc trạng thái isRead
                    if (isAuthenticated)
                        userId = GetCurrentUserId();
                }
            }

            List<NotificationDto> notifications;

            try
            {
                if (userId.HasValue && userId != null)
                {
                    notifications = await _context.NotificationReads
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
                } else
                {
                    // Nếu là public, không có user → lấy toàn bộ
                    notifications = await _context.Notifications
                        .OrderByDescending(n => n.createdAt)
                        .Select(n => new NotificationDto
                        {
                            notificationId = n.notificationId,
                            title = n.title,
                            body = n.body,
                            imageUrl = n.imageUrl,
                            type = n.type,
                            createdAt = n.createdAt,
                            data = n.data,
                            realEstateId = n.realEstateId
                        })
                        .ToListAsync();
                }

                    //var userId = GetCurrentUserId();
                    //var notifications = await _context.NotificationReads
                    //    .Include(nr => nr.Notifications)
                    //    .Where(nr => nr.userId == userId)
                    //    .OrderByDescending(nr => nr.Notifications!.createdAt)
                    //    .Select(nr => new NotificationDto
                    //    {
                    //        notificationId = nr.notificationId,
                    //        title = nr.Notifications!.title,
                    //        body = nr.Notifications.body,
                    //        imageUrl = nr.Notifications.imageUrl,
                    //        type = nr.Notifications.type,
                    //        createdAt = nr.Notifications.createdAt,
                    //        data = nr.Notifications.data,
                    //        isRead = nr.isRead,
                    //        realEstateId = nr.Notifications.realEstateId
                    //    })
                    //    .ToListAsync();

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

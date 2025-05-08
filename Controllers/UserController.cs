using Microsoft.AspNetCore.Mvc;
using RealEstateApi.Models;
using RealEstateApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using RealEstateApi.DOTs;
using System.Text.RegularExpressions;

namespace RealEstateApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public UserController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        //[HttpPost("register")]
        //public async Task<IActionResult> Register(User user)
        //{
        //    if (await _context.Users.AnyAsync(u => u.PhoneNumber == user.PhoneNumber))
        //        return BadRequest("Phone already registered");

        //    user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
        //    _context.Users.Add(user);
        //    await _context.SaveChangesAsync();
        //    return Ok(user);
        //}

        //[HttpPost("login")]
        //public async Task<IActionResult> Login(User login)
        //{
        //    var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == login.PhoneNumber);
        //    if (user == null)
        //        //if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.Password))
        //        //return Unauthorized("Sai số điện thoại hoặc mật khẩu.");
        //        //return Ok(new ApiResponse<object>
        //        //{
        //        //    success = false,
        //        //    model = null,
        //        //    message = "Sai thông tin đăng nhập",
        //        //    code = 401
        //        //});
        //        return Ok(ApiResult.Error<object>("Sai thông tin đăng nhập", 401));

        //    //return Ok(user);
        //    //return Ok(new ApiResponse<object>
        //    //{
        //    //    success = true,
        //    //    model = user,
        //    //    message = "Đăng nhập thành công",
        //    //    code = 200
        //    //});
        //    return Ok(ApiResult.Success(user, "Đăng nhập thành công"));
        //}

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest login)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.phoneNumber == login.phoneNumber);
            
            // Lấy thông tin người dùng từ token
            //var deviceIdFromToken = User.Claims.FirstOrDefault(c => c.Type == "DeviceId")?.Value;
            //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (user == null || !BCrypt.Net.BCrypt.Verify(login.password, user.password))
            {
                return Unauthorized(ApiResult.Error<object>("Số điện thoại hoặc mật khẩu không đúng", 401));
            }
            //else
            //{
            //}

            try
            {
                // Nếu đã login từ thiết bị khác
                //if (user.isActive == true)
                //{
                //    // Kiểm tra DeviceId
                //    if (!string.IsNullOrEmpty(user.deviceId) && user.deviceId != login.deviceId)
                //    {
                //        // Nếu DeviceId khác thì không cho đăng nhập
                //        return Conflict(ApiResult.Error<object>("Tài khoản đã được đăng nhập trên thiết bị khác!", 409));
                //    }
                //    else
                //    {
                //        // Cho phép đăng nhập
                //        //user.DeviceId = login.DeviceId;
                //    }
                //}
                //else
                //{
                //    // Chưa đăng nhập lần nào => cho đăng nhập & lưu DeviceId
                //    //user.IsActive = true;
                //    //user.DeviceId = login.DeviceId;
                //}

                // Cho phép đăng nhập
                user.isActive = true;
                user.deviceId = login.deviceId;
                await _context.SaveChangesAsync();

                // Tạo JWT token
                var tokenRes = GenerateJwtToken(user);

                var userInfo = new
                {
                    user.userId,
                    user.fullName,
                    user.phoneNumber,
                    user.address,
                    user.role,
                    user.createdAt,
                    user.isActive,
                    user.deviceId,
                    token = tokenRes
                };

                return Ok(ApiResult.Success<object>(userInfo, "Đăng nhập thành công", 200));
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(ApiResult.Error<object>("Đăng nhập không thành công", 400));
            }
            catch (DbUpdateException)
            {
                return BadRequest(ApiResult.Error<object>("Đăng nhập không thành công", 400));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Error<object>($"Đăng nhập không thành công. Lỗi: {ex.Message}", 400));
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.phoneNumber == request.phoneNumber);

            if (user == null)
            {
                return NotFound(ApiResult.Error<object>("Không tìm thấy người dùng", 404));
            }

            try
            {
                user.isActive = false;
                user.deviceId = null;
                user.fcmToken = null;
                user.platform = null;
                await _context.SaveChangesAsync();

                return Ok(ApiResult.Success<object>("Đăng xuất thành công"));
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(ApiResult.Error<object>("Đăng xuất không thành công", 400));
            }
            catch (DbUpdateException)
            {
                return BadRequest(ApiResult.Error<object>("Đăng xuất không thành công", 400));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Error<object>($"Đăng xuất không thành công. Lỗi: {ex.Message}", 400));
            }
        }

        [Authorize]
        [HttpPost("register-device")]
        public async Task<IActionResult> RegisterDevice([FromBody] RegisterDeviceRequest request)
        {
            try
            {
                var userId = GetUserIdFromToken(); // bạn có thể lấy userId từ JWT
                var user = await _context.Users.FirstOrDefaultAsync(u => u.userId == userId);

                if (user == null)
                    return NotFound("Không tìm thấy người dùng.");

                user.fcmToken = request.fcmToken;
                user.platform = request.platform;

                await _context.SaveChangesAsync();

                return Ok(ApiResult.Success<object>("Đăng ký token fcm thành công."));
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(ApiResult.Error<object>("Cập nhật không thành công", 400));
            }
            catch (DbUpdateException)
            {
                return BadRequest(ApiResult.Error<object>("Cập nhật không thành công", 400));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Error<object>($"Cập nhật không thành công. Lỗi: {ex.Message}", 400));
            }
        }

        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddUser([FromBody] UserDTO userDto)
        {
            // Kiểm tra định dạng số điện thoại
            if (string.IsNullOrWhiteSpace(userDto.phoneNumber) ||
                !Regex.IsMatch(userDto.phoneNumber, @"^0\d{9,14}$"))
            {
                return BadRequest(ApiResult.Error<object>("Số điện thoại không hợp lệ!", 400));
            }

            // Kiểm tra xem số điện thoại đã tồn tại chưa
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.phoneNumber == userDto.phoneNumber);
            if (existingUser != null)
            {
                return BadRequest(ApiResult.Error<object>("Số điện thoại đã tồn tại!", 400));
            }

            try
            {
                // Hash mật khẩu trước khi lưu
                //user.password = BCrypt.Net.BCrypt.HashPassword(userDto.password);
                //user.isActive = false;
                //user.deviceId = null;
                //existingUser!.fullName = userDto.fullName;
                var user = new User
                {
                    fullName  = userDto.fullName,
                    phoneNumber  = userDto.phoneNumber,
                    password  = BCrypt.Net.BCrypt.HashPassword(userDto.password),
                    role = userDto.role,
                    address  = userDto.address,
                    createdAt = DateTime.UtcNow,
                    isActive  = false,
                    deviceId = userDto.deviceId                  
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(ApiResult.Success<object>(user, "Thêm người dùng thành công", 201));
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(ApiResult.Error<object>("Thêm người dùng không thành công", 400));
            }
            catch (DbUpdateException)
            {
                return BadRequest(ApiResult.Error<object>("Thêm người dùng không thành công", 400));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Error<object>($"Thêm người dùng không thành công. Lỗi: {ex.Message}", 400));
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDTO updatedUser)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(ApiResult.Error<object>("Không tìm thấy người dùng", 404));
            }

            try
            {
                user.fullName = updatedUser.fullName;
                //user.PhoneNumber = updatedUser.PhoneNumber;

                if (!string.IsNullOrWhiteSpace(updatedUser.password))
                    user.password = BCrypt.Net.BCrypt.HashPassword(updatedUser.password);

                user.role = updatedUser.role;
                user.address = updatedUser.address;

                await _context.SaveChangesAsync();

                return Ok(ApiResult.Success<object>(user, "Cập nhật thành công", 200));
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(ApiResult.Error<object>("Cập nhật không thành công", 400));
            }
            catch (DbUpdateException)
            {
                return BadRequest(ApiResult.Error<object>("Cập nhật không thành công", 400));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Error<object>($"Cập nhật không thành công. Lỗi: {ex.Message}", 400));
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(ApiResult.Error<object>("Người dùng không tồn tại", 404));
            }

            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return Ok(ApiResult.Success<object>("Xóa người dùng thành công"));
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(ApiResult.Error<object>("Xóa người dùng không thành công", 400));
            }
            catch (DbUpdateException)
            {
                return BadRequest(ApiResult.Error<object>("Xóa người dùng không thành công", 400));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Error<object>($"Xóa người dùng không thành công. Lỗi: {ex.Message}", 400));
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        //[Authorize]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);

                if (user == null)
                    return NotFound(ApiResult.Error<object>("Không tìm thấy người dùng", 404));

                return Ok(ApiResult.Success<object>(user, "Kết quả tìm kiếm", 200));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Error<object>($"Không tìm thấy người dùng. Lỗi: {ex.Message}", 400));
            }
        }

        [Authorize]
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers(
            [FromQuery] string? search,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 10;

            try
            {
                var query = _context.Users.AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    string keyword = search.ToLower();
                    query = query.Where(u =>
                        (u.fullName ?? "").ToLower().Contains(keyword) ||
                        (u.phoneNumber ?? "").ToLower().Contains(keyword) ||
                        (u.address ?? "").ToLower().Contains(keyword)
                    );
                }

                // Tổng số bản ghi
                int totalRecords = await query.CountAsync();

                // Phân trang
                var result = await query
                    .OrderByDescending(r => r.createdAt)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new
                    {
                        u.userId,
                        u.fullName,
                        u.phoneNumber,
                        u.address,
                        u.role,
                        u.createdAt,
                        u.isActive,
                        u.deviceId
                    })
                    .ToListAsync();

                var res = new
                {
                    success = true,
                    items = result,
                    pageIndex = pageIndex,
                    totalRecords = totalRecords,
                    message = "Kết quả tìm kiếm",
                    code = 200
                };

                return Ok(res);

                //return Ok(ApiResult.Success<object>(result, "Kết quả tìm kiếm", 200));
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(ApiResult.Error<object>("Tìm kiếm không thành công", 400));
            }
            catch (DbUpdateException)
            {
                return BadRequest(ApiResult.Error<object>("Tìm kiếm không thành công", 400));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Error<object>($"Tìm kiếm không thành công. Lỗi: {ex.Message}", 400));
            }
        }

        //public async Task<IActionResult> SearchUsers(
        //    [FromQuery] string? fullName,
        //    [FromQuery] string? phoneNumber,
        //    [FromQuery] string? address,
        //    [FromQuery] int? role)
        //{
        //    var query = _context.Users.AsQueryable();

        //    try
        //    {
        //        if (!string.IsNullOrWhiteSpace(fullName))
        //        {
        //            query = query.Where(u => (u.fullName ?? "").ToLower().Contains(fullName.ToLower()));
        //        }

        //        if (!string.IsNullOrWhiteSpace(phoneNumber))
        //        {
        //            query = query.Where(u => u.phoneNumber.Contains(phoneNumber));
        //        }

        //        if (!string.IsNullOrWhiteSpace(address))
        //        {
        //            query = query.Where(u => (u.address ?? "").ToLower().Contains(address.ToLower()));
        //        }

        //        if (role.HasValue)
        //        {
        //            query = query.Where(u => u.role == role);
        //        }

        //        var result = await query
        //            .Select(u => new
        //            {
        //                u.userId,
        //                u.fullName,
        //                u.phoneNumber,
        //                u.address,
        //                u.role,
        //                u.createdAt,
        //                u.isActive,
        //                u.deviceId
        //            }).ToListAsync();

        //        return Ok(ApiResult.Success<object>(result, "Kết quả tìm kiếm", 200));
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        return BadRequest(ApiResult.Error<object>("Tìm kiếm không thành công", 400));
        //    }
        //    catch (DbUpdateException)
        //    {
        //        return BadRequest(ApiResult.Error<object>("Tìm kiếm không thành công", 400));
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ApiResult.Error<object>($"Tìm kiếm không thành công. Lỗi: {ex.Message}", 400));
        //    }
        //}

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.userId.ToString()),
                    new Claim(ClaimTypes.Name, user.phoneNumber ?? string.Empty),
                    new Claim("deviceId", user.deviceId ?? ""),
                    new Claim("role", user.role.ToString())
                };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(Convert.ToDouble(jwtSettings["ExpireDays"])),
                //expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpireMinutes"])),
                //expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private int GetUserIdFromToken()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString))
                throw new UnauthorizedAccessException("Invalid token: user ID not found");
            return int.Parse(userIdString);
        }
    }
}

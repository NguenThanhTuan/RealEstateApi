using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApi.Data;
using RealEstateApi.Models;
using Serilog;

namespace RealEstateApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppConfig : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AppConfig(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        //[Authorize]
        [HttpGet("config")]
        public IActionResult GetShowNewsScreen()
        {
            //return Ok(new { show = showNews == "true" });
            try
            {
                // Lấy từ DB (hoặc có thể từ appsettings.json)
                var showNews = _context.AppConfigs
                    .FirstOrDefault(x => x.key == "NewsScreen")?.value;

                var show = showNews == "true";

                // Kiểm tra User-Agent để xác định thiết bị Android
                var userAgent = Request.Headers["User-Agent"].ToString();
                var isAndroid = userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase);

                Log.Information("User-Agent: {UserAgent}", userAgent);
                Log.Information("User-Agent: " + userAgent);
                //Log.Information("IsAuthenticated: " + isAuthenticated);
                Log.Information("IsAndroid: " + isAndroid);
                foreach (var header in Request.Headers)
                {
                    Log.Information("Header: {Key} = {Value}", header.Key, header.Value.ToString());
                }

                // Lấy config Android
                var showNewsAndr = _context.AppConfigs
                    .FirstOrDefault(x => x.key == "NewsScreenAndr")?.value;

                var showAndr = showNewsAndr == "true";

                var res = new
                {
                    success = true,
                    show = isAndroid ? (showNewsAndr == "true") : (showNews == "true"),
                    message = "Kết quả",
                    code = 200
                };

                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Error<object>($"Lỗi: {ex.Message}", 400));
            }
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateNewsScreenShow([FromBody] UpdateAppConfigDto dto)
        {
            var config = await _context.AppConfigs.FirstOrDefaultAsync(c => c.key == "NewsScreen");

            if (config == null)
            {
                // Nếu chưa có thì tạo mới
                config = new AppConfigM
                {
                    key = "NewsScreen",
                    value = dto.value.ToString().ToLower()
                };
                _context.AppConfigs.Add(config);
            }
            else
            {
                // Nếu có thì cập nhật
                config.value = dto.value.ToString().ToLower();
                _context.AppConfigs.Update(config);
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Cập nhật thành công" });
        }

    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApi.Data;
using RealEstateApi.Models;
using System.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RealEstateApi.Controllers
{
    [Route("api/admin-units")]
    [ApiController]
    public class LocationController : Controller
    {
        private readonly AppDbContext _context;

        public LocationController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/admin-units/provinces
        [HttpGet("provinces")]
        public async Task<IActionResult> GetProvinces()
        {
            var provinces = await _context.Provinces.ToListAsync();
            return Ok(ApiResult.Success<object>(provinces, "Danh sách tỉnh/thành phố", 200));
        }

        // GET: api/admin-units/districts?provinceId=1
        [HttpGet("districts")]
        public async Task<IActionResult> GetDistricts(int provinceId)
        {
            var districts = await _context.Districts
                .Where(d => d.provinceId == provinceId)
                .ToListAsync();

            if (districts == null)
            {
                return NotFound(ApiResult.Error<object>("Không có dữ liệu quận/huyện", 404));
            }

            return Ok(ApiResult.Success<object>(districts, "Danh sách quận/huyện", 200));
        }

        // GET: api/admin-units/wards?districtId=2
        [HttpGet("wards")]
        public async Task<IActionResult> GetWards(int districtId)
        {
            var wards = await _context.Wards
                .Where(w => w.districtId == districtId)
                .ToListAsync();

            if (wards == null)
            {
                return NotFound(ApiResult.Error<object>("Không có dữ liệu phường/xã", 404));
            }

            return Ok(ApiResult.Success<object>(wards, "Danh sách quận/huyện", 200));
        }

        // POST: api/admin-units/province
        [Authorize]
        [HttpPost("province/add")]
        public async Task<IActionResult> CreateProvince([FromBody] Province model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.name))
                {
                    return BadRequest(ApiResult.Error<object>("Tên tỉnh/thành phố không được để trống", 400));
                }
                // Kiểm tra trùng tên (không phân biệt hoa thường)
                var existing = await _context.Provinces
                    .FirstOrDefaultAsync(p => p.name.ToLower() == model.name.ToLower());

                if (existing != null)
                {
                    return Conflict(ApiResult.Error<object>("Tỉnh/thành phố đã tồn tại", 409));
                }

                _context.Provinces.Add(model);
                await _context.SaveChangesAsync();

                return Ok(ApiResult.Success<object>(model, "Thêm mới thành công", 201));
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(ApiResult.Error<object>("Thêm mới không thành công", 400));
            }
            catch (DbUpdateException)
            {
                return BadRequest(ApiResult.Error<object>("Thêm mới không thành công", 400));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Error<object>($"Thêm mới không thành công. Lỗi: {ex.Message}", 400));
            }
        }

        [Authorize]
        [HttpPost("district/add")]
        public async Task<IActionResult> CreateDistrict([FromBody] District model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.name))
                {
                    return BadRequest(ApiResult.Error<object>("Tên quận huyện không được để trống", 400));
                }
                // Kiểm tra trùng tên (không phân biệt hoa thường)
                var existing = await _context.Districts
                    .FirstOrDefaultAsync(p => p.name.ToLower() == model.name.ToLower());

                if (existing != null)
                {
                    return Conflict(ApiResult.Error<object>("Quận huyện đã tồn tại", 409));
                }

                _context.Districts.Add(model);
                await _context.SaveChangesAsync();

                return Ok(ApiResult.Success<object>(model, "Thêm mới thành công", 201));
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(ApiResult.Error<object>("Thêm mới không thành công", 400));
            }
            catch (DbUpdateException)
            {
                return BadRequest(ApiResult.Error<object>("Thêm mới không thành công", 400));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Error<object>($"Thêm mới không thành công. Lỗi: {ex.Message}", 400));
            }
        }

        [Authorize]
        [HttpPost("ward/add")]
        public async Task<IActionResult> CreateWard([FromBody] Ward model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.name))
                {
                    return BadRequest(ApiResult.Error<object>("Tên phường/xã không được để trống", 400));
                }
                // Kiểm tra trùng tên (không phân biệt hoa thường)
                var existing = await _context.Wards
                    .FirstOrDefaultAsync(p => p.name.ToLower() == model.name.ToLower());

                if (existing != null)
                {
                    return Conflict(ApiResult.Error<object>("Phường/xã đã tồn tại", 409));
                }

                _context.Wards.Add(model);
                await _context.SaveChangesAsync();

                return Ok(ApiResult.Success<object>(model, "Thêm mới thành công", 201));
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(ApiResult.Error<object>("Thêm mới không thành công", 400));
            }
            catch (DbUpdateException)
            {
                return BadRequest(ApiResult.Error<object>("Thêm mới không thành công", 400));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Error<object>($"Thêm mới không thành công. Lỗi: {ex.Message}", 400));
            }
        }

        // PUT: api/admin-units/province/2
        [Authorize]
        [HttpPut("province/{id}")]
        public async Task<IActionResult> UpdateProvince(int id, [FromBody] Province model)
        {
            var existing = await _context.Provinces.FindAsync(id);
            if (existing == null)
                return NotFound(ApiResult.Error<object>("Không tìm thấy", 404));

            try
            {
                existing.name = model.name;
                await _context.SaveChangesAsync();

                return Ok(ApiResult.Success<object>(existing, "Cập nhật thành công", 200));
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

        // PUT: api/admin-units/district/2
        [Authorize]
        [HttpPut("district/{id}")]
        public async Task<IActionResult> UpdateDistrict(int id, [FromBody] District model)
        {
            var existing = await _context.Districts.FindAsync(id);
            if (existing == null)
                return NotFound(ApiResult.Error<object>("Không tìm thấy", 404));

            try
            {
                existing.name = model.name;
                existing.provinceId = model.provinceId;
                await _context.SaveChangesAsync();

                return Ok(ApiResult.Success<object>(existing, "Cập nhật thành công", 200));
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

        // PUT: api/admin-units/district/2
        [Authorize]
        [HttpPut("ward/{id}")]
        public async Task<IActionResult> UpdateWard(int id, [FromBody] Ward model)
        {
            var existing = await _context.Wards.FindAsync(id);
            if (existing == null)
                return NotFound(ApiResult.Error<object>("Không tìm thấy", 404));

            try
            {
                existing.name = model.name;
                existing.districtId = model.districtId;
                await _context.SaveChangesAsync();

                return Ok(ApiResult.Success<object>(existing, "Cập nhật thành công", 200));
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

        // DELETE: api/admin-units/province/2
        [Authorize]
        [HttpDelete("province/{id}")]
        public async Task<IActionResult> DeleteProvince(int id)
        {
            var province = await _context.Provinces.FindAsync(id);
            if (province == null)
                return NotFound(ApiResult.Error<object>("Tỉnh/thành phố không tồn tại", 404));

            try
            {
                _context.Provinces.Remove(province);
                await _context.SaveChangesAsync();

                return Ok(ApiResult.Success<object>("Xóa thành công"));
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(ApiResult.Error<object>("Xóa không thành công", 400));
            }
            catch (DbUpdateException)
            {
                return BadRequest(ApiResult.Error<object>("Xóa không thành công", 400));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Error<object>($"Xóa không thành công. Lỗi: {ex.Message}", 400));
            }
        }

        // DELETE: api/admin-units/district/2
        [Authorize]
        [HttpDelete("district/{id}")]
        public async Task<IActionResult> DeleteDistrict(int id)
        {
            var district = await _context.Districts.FindAsync(id);
            if (district == null)
                return NotFound(ApiResult.Error<object>("Quận huyện không tồn tại", 404));
            try
            {
                _context.Districts.Remove(district);
                await _context.SaveChangesAsync();
                return Ok(ApiResult.Success<object>("Xóa thành công"));
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(ApiResult.Error<object>("Xóa không thành công", 400));
            }
            catch (DbUpdateException)
            {
                return BadRequest(ApiResult.Error<object>("Xóa không thành công", 400));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Error<object>($"Xóa không thành công. Lỗi: {ex.Message}", 400));
            }
        }

        // DELETE: api/admin-units/ward/2
        [Authorize]
        [HttpDelete("ward/{id}")]
        public async Task<IActionResult> DeleteWard(int id)
        {
            var ward = await _context.Wards.FindAsync(id);
            if (ward == null)
                return NotFound(ApiResult.Error<object>("Phường/xã không tồn tại", 404));
            try
            {
                _context.Wards.Remove(ward);
                await _context.SaveChangesAsync();
                return Ok(ApiResult.Success<object>("Xóa thành công"));
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(ApiResult.Error<object>("Xóa không thành công", 400));
            }
            catch (DbUpdateException)
            {
                return BadRequest(ApiResult.Error<object>("Xóa không thành công", 400));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Error<object>($"Xóa không thành công. Lỗi: {ex.Message}", 400));
            }
        }

    }
}

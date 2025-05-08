using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateApi.Data;
using RealEstateApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Xml;
using RealEstateApi.DOTs;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace RealEstateApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RealEstateController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public RealEstateController(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        //[Authorize]
        //[HttpPost("add-info")]
        //public async Task<IActionResult> CreateRealEstate([FromBody] RealEstateCreateRequest2 request)
        //{
        //    try
        //    {
        //        if (request == null)
        //        {
        //            return BadRequest(ApiResult.Error<object>("Dữ liệu không hợp lệ", 400));
        //        }
        //        if (request.price < 0)
        //        {
        //            return BadRequest(ApiResult.Error<object>("Giá bất hợp lệ", 400));
        //        }
        //        if (request.area < 0)
        //        {
        //            return BadRequest(ApiResult.Error<object>("Diện tích không hợp lệ", 400));
        //        }
        //        if (request.floors < 0)
        //        {
        //            return BadRequest(ApiResult.Error<object>("Số tầng không hợp lệ", 400));
        //        }
        //        if (request.bedrooms < 0)
        //        {
        //            return BadRequest(ApiResult.Error<object>("Số phòng ngủ không hợp lệ", 400));
        //        }
        //        if (request.bathrooms < 0)
        //        {
        //            return BadRequest(ApiResult.Error<object>("Số phòng tắm không hợp lệ", 400));
        //        }
        //        if (request.length < 0)
        //        {
        //            return BadRequest(ApiResult.Error<object>("Chiều dài không hợp lệ", 400));
        //        }
        //        if (request.width < 0)
        //        {
        //            return BadRequest(ApiResult.Error<object>("Chiều rộng không hợp lệ", 400));
        //        }
        //        if (request.roadWidth < 0)
        //        {
        //            return BadRequest(ApiResult.Error<object>("Độ rộng của đường không hợp lệ", 400));
        //        }
        //        var estate = new RealEstates
        //        {
        //            name = request.name,
        //            price = request.price,
        //            province = request.province,
        //            district = request.district,
        //            ward = request.ward,
        //            street = request.street,
        //            category = request.category,
        //            status = request.status,
        //            area = request.area,
        //            roadWidth = request.roadWidth,
        //            floors = request.floors,
        //            bedrooms = request.bedrooms,
        //            bathrooms = request.bathrooms,
        //            direction = request.direction,
        //            description = request.description,
        //            length = request.length,
        //            width = request.width
        //        };

        //        _context.RealEstates.Add(estate);
        //        await _context.SaveChangesAsync();

        //        return Ok(ApiResult.Success(estate, "Tạo bất động sản thành công", 200));
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        return BadRequest(ApiResult.Error<object>("Thêm bất động sản không thành công", 400));
        //    }
        //    catch (DbUpdateException)
        //    {
        //        return BadRequest(ApiResult.Error<object>("Thêm bất động sản không thành công", 400));
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ApiResult.Error<object>($"Lỗi: {ex.Message}", 500));
        //    }
        //}

        //[Authorize]
        //[HttpPost("{id}/upload-images")]
        //public async Task<IActionResult> UploadImages(int id, [FromForm] List<IFormFile> images)
        //{
        //    // Xử lý ảnh
        //    List<string> imageUrls = new();
        //    var estate = await _context.RealEstates.FindAsync(id);
        //    if (estate == null)
        //    {
        //        return NotFound(ApiResult.Error<object>("Không tìm thấy bất động sản", 401));
        //    }

        //    if (images == null || images.Count < 4)
        //    {
        //        return BadRequest(ApiResult.Error<object>("Vui lòng chọn ít nhất 4 ảnh", 400));
        //    }

        //    var imageEntities = new List<RealEstateImages>();

        //    var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        //    var estateFolder = Path.Combine(webRootPath, "images"); // estate.realEstateId.ToString()
        //    if (!Directory.Exists(estateFolder))
        //    {
        //        Directory.CreateDirectory(estateFolder);
        //    }

        //    foreach (var file in images)
        //    {
        //        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        //        var filePath = Path.Combine("wwwroot/images", fileName);

        //        using (var stream = new FileStream(filePath, FileMode.Create))
        //        {
        //            await file.CopyToAsync(stream);
        //        }

        //        var imageUrl = $"/images/{fileName}";
        //        imageUrls.Add(imageUrl);

        //        imageEntities.Add(new RealEstateImages
        //        {
        //            realEstateId = id,
        //            imageUrl = imageUrl
        //        });
        //    }

        //    _context.RealEstateImages.AddRange(imageEntities);
        //    await _context.SaveChangesAsync();

        //    return Ok(ApiResult.Success<object>(imageUrls, "Tải ảnh thành công", 200));
        //}

        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> CreateRealEstate([FromForm] RealEstateCreateRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (request == null)
                {
                    return BadRequest(ApiResult.Error<object>("Dữ liệu không hợp lệ", 400));
                }
                if (request.price < 0)
                {
                    return BadRequest(ApiResult.Error<object>("Giá bất hợp lệ", 400));
                }
                if (request.area < 0)
                {
                    return BadRequest(ApiResult.Error<object>("Diện tích không hợp lệ", 400));
                }
                if (request.floors < 0)
                {
                    return BadRequest(ApiResult.Error<object>("Số tầng không hợp lệ", 400));
                }
                if (request.bedrooms < 0)
                {
                    return BadRequest(ApiResult.Error<object>("Số phòng ngủ không hợp lệ", 400));
                }
                if (request.bathrooms < 0)
                {
                    return BadRequest(ApiResult.Error<object>("Số phòng tắm không hợp lệ", 400));
                }
                if (request.length < 0)
                {
                    return BadRequest(ApiResult.Error<object>("Chiều dài không hợp lệ", 400));
                }
                if (request.width < 0)
                {
                    return BadRequest(ApiResult.Error<object>("Chiều rộng không hợp lệ", 400));
                }
                if (request.roadWidth < 0)
                {
                    return BadRequest(ApiResult.Error<object>("Độ rộng của đường không hợp lệ", 400));
                }

                // Xử lý ảnh
                //List<string> imageUrls = new();

                var estate = new RealEstates
                {
                    name = request.name,
                    price = request.price,
                    province = request.province,
                    district = request.district,
                    ward = request.ward,
                    street = request.street,
                    category = request.category,
                    status = request.status,
                    area = request.area,
                    roadWidth = request.roadWidth,
                    floors = request.floors,
                    bedrooms = request.bedrooms,
                    bathrooms = request.bathrooms,
                    direction = request.direction,
                    description = request.description,
                    length = request.length,
                    width = request.width,
                    updatedDate = null
                };

                _context.RealEstates.Add(estate);
                await _context.SaveChangesAsync();

                List<RealEstateImages> savedImages = new();
                if (request.images != null && request.images.Any())
                {
                    var images = new List<RealEstateImages>();

                    var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var estateFolder = Path.Combine(webRootPath, "images"); // estate.realEstateId.ToString()
                    if (!Directory.Exists(estateFolder))
                    {
                        Directory.CreateDirectory(estateFolder);
                    }

                    foreach (var file in request.images)
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                        var filePath = Path.Combine("wwwroot/images", fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var imageUrl = $"/images/{fileName}";
                        //imageUrls.Add(imageUrl);

                        images.Add(new RealEstateImages
                        {
                            realEstateId = estate.realEstateId,
                            imageUrl = imageUrl
                        });
                    }

                    _context.RealEstateImages.AddRange(images);
                    await _context.SaveChangesAsync();

                    // Lưu lại danh sách ảnh vừa insert (có Id)
                    savedImages = images;
                }

                var result = new
                {
                    estate.realEstateId,
                    estate.name,
                    estate.price,
                    estate.province,
                    estate.district,
                    estate.ward,
                    estate.street,
                    estate.category,
                    estate.status,
                    estate.area,
                    estate.roadWidth,
                    estate.floors,
                    estate.bedrooms,
                    estate.bathrooms,
                    estate.direction,
                    estate.description,
                    estate.length,
                    estate.width,
                    estate.postedDate,
                    images = savedImages.Select(img => new
                    {
                        img.imageId,
                        img.imageUrl
                    })
                    //images = imageUrls
                };

                var dto = new RealEstateNotificationDto
                {
                    realEstateId = estate.realEstateId,
                    name = estate.name,
                    price = estate.price,
                    province = estate.province,
                    district = estate.district,
                    ward = estate.ward,
                    street = estate.street,
                    category = estate.category,
                    status = estate.status,
                    area = estate.area,
                    roadWidth = estate.roadWidth,
                    floors = estate.floors,
                    bedrooms = estate.bedrooms,
                    bathrooms = estate.bathrooms,
                    direction = estate.direction!,
                    description = estate.description!,
                    length = estate.length,
                    width = estate.width,
                    postedDate = estate.postedDate,
                    images = savedImages.Select(img => new RealEstateNotificationDto.ImageDto
                    {
                        imageId = img.imageId,
                        imageUrl = img.imageUrl
                    }).ToList()
                };

                await _notificationService.CreateAndSendNotificationAsync(1, dto, currentUserId);
                return Ok(ApiResult.Success<object>(result, "Tạo bất động sản thành công", 200));
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(ApiResult.Error<object>("Thêm bất động sản không thành công", 400));
            }
            catch (DbUpdateException)
            {
                return BadRequest(ApiResult.Error<object>("Thêm bất động sản không thành công", 400));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Error<object>($"Lỗi: {ex.Message}", 500));
            }
        }

    //[Authorize]
        //[HttpPut("update-info/{id}")]
        //public async Task<IActionResult> UpdateRealEstateInfo(int id, [FromBody] RealEstateUpdateInfoRequest request)
        //{
        //    try
        //    {
        //        if (request == null)
        //        {
        //            return BadRequest(ApiResult.Error<object>("Dữ liệu không hợp lệ", 400));
        //        }
        //        if (request.price < 0)
        //        {
        //            return BadRequest(ApiResult.Error<object>("Giá bất hợp lệ", 400));
        //        }
        //        if (request.area < 0)
        //        {
        //            return BadRequest(ApiResult.Error<object>("Diện tích không hợp lệ", 400));
        //        }
        //        if (request.floors < 0)
        //        {
        //            return BadRequest(ApiResult.Error<object>("Số tầng không hợp lệ", 400));
        //        }
        //        if (request.bedrooms < 0)
        //        {
        //            return BadRequest(ApiResult.Error<object>("Số phòng ngủ không hợp lệ", 400));
        //        }
        //        if (request.bathrooms < 0)
        //        {
        //            return BadRequest(ApiResult.Error<object>("Số phòng tắm không hợp lệ", 400));
        //        }
        //        if (request.length < 0)
        //        {
        //            return BadRequest(ApiResult.Error<object>("Chiều dài không hợp lệ", 400));
        //        }
        //        if (request.width < 0)
        //        {
        //            return BadRequest(ApiResult.Error<object>("Chiều rộng không hợp lệ", 400));
        //        }
        //        if (request.roadWidth < 0)
        //        {
        //            return BadRequest(ApiResult.Error<object>("Độ rộng của đường không hợp lệ", 400));
        //        }

        //        var estate = await _context.RealEstates.FindAsync(id);
        //        if (estate == null)
        //            return NotFound(ApiResult.Error<object>("Không tìm thấy bất động sản", 404));

        //        // Cập nhật thông tin
        //        estate.name = request.name;
        //        estate.price = request.price;
        //        estate.province = request.province;
        //        estate.district = request.district;
        //        estate.ward = request.ward;
        //        estate.street = request.street;
        //        estate.category = request.category;
        //        estate.status = request.status;
        //        estate.area = request.area;
        //        estate.roadWidth = request.roadWidth;
        //        estate.floors = request.floors;
        //        estate.bedrooms = request.bedrooms;
        //        estate.bathrooms = request.bathrooms;
        //        estate.direction = request.direction;
        //        estate.description = request.description;
        //        estate.length = request.length;
        //        estate.width = request.width;
        //        estate.postedDate = request.postedDate;

        //        await _context.SaveChangesAsync();
        //        return Ok(ApiResult.Success<object>(estate, "Cập nhật bất động sản thành công", 200));

        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        return BadRequest(ApiResult.Error<object>("Cập nhật không thành công", 400));
        //    }
        //    catch (DbUpdateException)
        //    {
        //        return BadRequest(ApiResult.Error<object>("Cập nhật không thành công", 400));
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ApiResult.Error<object>($"Cập nhật không thành công. Lỗi: {ex.Message}", 500));
        //    }  
        //}

        //[Authorize]
        //[HttpPut("update-images/{id}")]
        //public async Task<IActionResult> UpdateRealEstateImages(int id, [FromForm] RealEstateUpdateImagesRequest request)
        //{
        //    var estate = await _context.RealEstates
        //        .Include(r => r.images)
        //        .FirstOrDefaultAsync(r => r.realEstateId == id);

        //    if (estate == null)
        //        return NotFound(ApiResult.Error<object>("Không tìm thấy bất động sản", 404));

        //    // Xoá ảnh cũ
        //    if (request.imageIdsToDelete != null && request.imageIdsToDelete.Any())
        //    {
        //        var toDelete = estate.images
        //            .Where(i => request.imageIdsToDelete.Contains(i.imageId)).ToList();

        //        foreach (var img in toDelete)
        //        {
        //            var path = Path.Combine("wwwroot", img.imageUrl.TrimStart('/'));
        //            if (System.IO.File.Exists(path))
        //                System.IO.File.Delete(path);
        //        }

        //        _context.RealEstateImages.RemoveRange(toDelete);
        //    }

        //    // Thêm ảnh mới
        //    if (request.newImages != null && request.newImages.Any())
        //    {
        //        var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        //        var estateFolder = Path.Combine(webRootPath, "images"); // estate.realEstateId.ToString()
        //        if (!Directory.Exists(estateFolder))
        //        {
        //            Directory.CreateDirectory(estateFolder);
        //        }

        //        foreach (var file in request.newImages)
        //        {
        //            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        //            var filePath = Path.Combine("wwwroot/images", fileName);
        //            using (var stream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await file.CopyToAsync(stream);
        //            }

        //            _context.RealEstateImages.Add(new RealEstateImages
        //            {
        //                realEstateId = id,
        //                imageUrl = $"/images/{fileName}"
        //            });
        //        }
        //    }

        //    await _context.SaveChangesAsync();

        //    var updatedImages = await _context.RealEstateImages.Where(i => i.realEstateId == id)
        //        .Select(i => new
        //        {
        //            i.imageId,
        //            i.imageUrl
        //        }).ToListAsync();

        //    return Ok(ApiResult.Success<object>(updatedImages, "Cập nhật ảnh thành công", 200));
        //}

        [Authorize]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateRealEstate(int id, [FromForm] RealEstateUpdateRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (request == null)
                {
                    return BadRequest(ApiResult.Error<object>("Dữ liệu không hợp lệ", 400));
                }
                if (request.price < 0)
                {
                    return BadRequest(ApiResult.Error<object>("Giá bất hợp lệ", 400));
                }
                if (request.area < 0)
                {
                    return BadRequest(ApiResult.Error<object>("Diện tích không hợp lệ", 400));
                }
                if (request.floors < 0)
                {
                    return BadRequest(ApiResult.Error<object>("Số tầng không hợp lệ", 400));
                }
                if (request.bedrooms < 0)
                {
                    return BadRequest(ApiResult.Error<object>("Số phòng ngủ không hợp lệ", 400));
                }
                if (request.bathrooms < 0)
                {
                    return BadRequest(ApiResult.Error<object>("Số phòng tắm không hợp lệ", 400));
                }
                if (request.length < 0)
                {
                    return BadRequest(ApiResult.Error<object>("Chiều dài không hợp lệ", 400));
                }
                if (request.width < 0)
                {
                    return BadRequest(ApiResult.Error<object>("Chiều rộng không hợp lệ", 400));
                }
                if (request.roadWidth < 0)
                {
                    return BadRequest(ApiResult.Error<object>("Độ rộng của đường không hợp lệ", 400));
                }

                //var estate = await _context.RealEstates.FindAsync(id);
                var estate = await _context.RealEstates.Include(r => r.images).FirstOrDefaultAsync(r => r.realEstateId == id);

                if (estate == null)
                    return NotFound(ApiResult.Error<object>("Không tìm thấy bất động sản", 404));

                // Cập nhật thông tin
                estate.name = request.name;
                estate.price = request.price;
                estate.province = request.province;
                estate.district = request.district;
                estate.ward = request.ward;
                estate.street = request.street;
                estate.category = request.category;
                estate.status = request.status;
                estate.area = request.area;
                estate.roadWidth = request.roadWidth;
                estate.floors = request.floors;
                estate.bedrooms = request.bedrooms;
                estate.bathrooms = request.bathrooms;
                estate.direction = request.direction;
                estate.description = request.description;
                estate.length = request.length;
                estate.width = request.width;
                estate.updatedDate = DateTime.UtcNow;

                // Xử lý ảnh
                if (request.imageIdsToDelete != null && request.imageIdsToDelete.Any())
                {
                    var imagesToDelete = _context.RealEstateImages
                        .Where(i => request.imageIdsToDelete.Contains(i.imageId) && i.realEstateId == id);

                    foreach (var img in imagesToDelete)
                    {
                        var fullPath = Path.Combine("wwwroot", img.imageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(fullPath))
                            System.IO.File.Delete(fullPath);
                    }

                    _context.RealEstateImages.RemoveRange(imagesToDelete);
                }

                if (request.newImages != null && request.newImages.Any())
                {
                    var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var estateFolder = Path.Combine(webRootPath, "images"); // estate.realEstateId.ToString()
                    if (!Directory.Exists(estateFolder))
                    {
                        Directory.CreateDirectory(estateFolder);
                    }

                    foreach (var file in request.newImages)
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                        var filePath = Path.Combine("wwwroot/images", fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var imageUrl = $"/images/{fileName}";

                        _context.RealEstateImages.Add(new RealEstateImages
                        {
                            realEstateId = id,
                            imageUrl = imageUrl
                        });
                    }
                }

                await _context.SaveChangesAsync();

                var result = new
                {
                    estate.realEstateId,
                    estate.name,
                    estate.price,
                    estate.province,
                    estate.district,
                    estate.ward,
                    estate.street,
                    estate.category,
                    estate.status,
                    estate.area,
                    estate.roadWidth,
                    estate.floors,
                    estate.bedrooms,
                    estate.bathrooms,
                    estate.direction,
                    estate.description,
                    estate.length,
                    estate.width,
                    estate.postedDate,
                    estate.updatedDate,
                    images = estate.images.Select(img => new
                    {
                        img.imageId,
                        img.imageUrl
                    })
                };

                var dto = new RealEstateNotificationDto
                {
                    realEstateId = estate.realEstateId,
                    name = estate.name,
                    price = estate.price,
                    province = estate.province,
                    district = estate.district,
                    ward = estate.ward,
                    street = estate.street,
                    category = estate.category,
                    status = estate.status,
                    area = estate.area,
                    roadWidth = estate.roadWidth,
                    floors = estate.floors,
                    bedrooms = estate.bedrooms,
                    bathrooms = estate.bathrooms,
                    direction = estate.direction!,
                    description = estate.description!,
                    length = estate.length,
                    width = estate.width,
                    postedDate = estate.postedDate,
                    updatedDate = estate.updatedDate ?? DateTime.UtcNow,
                    images = estate.images.Select(img => new RealEstateNotificationDto.ImageDto
                    {
                        imageId = img.imageId,
                        imageUrl = img.imageUrl
                    }).ToList()
                };

                await _notificationService.CreateAndSendNotificationAsync(2, dto, currentUserId);

                return Ok(ApiResult.Success<object>(result, "Cập nhật bất động sản thành công", 200));

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
                return BadRequest(ApiResult.Error<object>($"Cập nhật không thành công. Lỗi: {ex.Message}", 500));
            }
        }

        [Authorize]
        [HttpPut("update-status/{id}")]
        //public async Task<IActionResult> UpdateStatus(int id, [FromBody] string newStatus)
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                var estate = await _context.RealEstates.FindAsync(id);
                if (estate == null)
                {
                    return NotFound(ApiResult.Error<object>("Bất động sản không tồn tại", 404));
                }

                //estate.status = newStatus;
                estate.status = request.status;
                _context.RealEstates.Update(estate);
                await _context.SaveChangesAsync();

                return Ok(ApiResult.Success<object>(estate, "Cập nhật trạng thái thành công", 200));
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
                return BadRequest(ApiResult.Error<object>($"Cập nhật không thành công. Lỗi: {ex.Message}", 500));
            }
        }

        // [Authorize(Roles = "1")] // Ví dụ: chỉ role = 1 (admin) được xoá
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRealEstate(int id)
        {
            try
            {
                var realEstate = await _context.RealEstates
                    .Include(r => r.images)
                    .FirstOrDefaultAsync(r => r.realEstateId == id);

                if (realEstate == null)
                {
                    return NotFound(ApiResult.Error<object>("Bất động sản không tồn tại", 404));
                }

                // Xóa ảnh nếu có
                if (realEstate.images != null && realEstate.images.Any())
                {
                    foreach (var image in realEstate.images)
                    {
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.imageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath); // XÓA FILE ẢNH VẬT LÝ
                        }
                    }

                    _context.RealEstateImages.RemoveRange(realEstate.images);
                }

                // Xóa bất động sản
                _context.RealEstates.Remove(realEstate);
                await _context.SaveChangesAsync();

                return Ok(ApiResult.Success<object>("Xoá bất động sản thành công"));
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(ApiResult.Error<object>("Xoá bất động sản không thành công", 400));
            }
            catch (DbUpdateException)
            {
                return BadRequest(ApiResult.Error<object>("Xoá bất động sản không thành công", 400));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Error<object>($"Xoá bất động sản không thành công. Lỗi: {ex.Message}", 400));
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRealEstateById(int id)
        {
            try
            {
                var realEstate = await _context.RealEstates
                    .Include(r => r.images)
                    .FirstOrDefaultAsync(r => r.realEstateId == id);

                if (realEstate == null)
                {
                    return NotFound(ApiResult.Error<object>("Bất động sản không tồn tại", 404));
                }

                var result = new
                {
                    realEstate.realEstateId,
                    realEstate.name,
                    realEstate.price,
                    realEstate.province,
                    realEstate.district,
                    realEstate.ward,
                    realEstate.street,
                    realEstate.category,
                    realEstate.status,
                    realEstate.area,
                    realEstate.roadWidth,
                    realEstate.floors,
                    realEstate.bedrooms,
                    realEstate.bathrooms,
                    realEstate.direction,
                    realEstate.description,
                    realEstate.length,
                    realEstate.width,
                    realEstate.postedDate,
                    images = realEstate.images.Select(i => new 
                    {
                        i.imageId,
                        i.imageUrl
                    }).ToList()
                };

                return Ok(ApiResult.Success<object>(result, "Kết quả tìm kiếm", 200));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Error<object>($"Bất động sản không tồn tại. Lỗi: {ex.Message}", 400));
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await _context.RealEstates
                .Include(r => r.images)
                .OrderByDescending(r => r.postedDate)
                .Select(r => new
                {
                    r.realEstateId,
                    r.name,
                    r.price,
                    r.province,
                    r.district,
                    r.ward,
                    r.street,
                    r.category,
                    r.status,
                    r.area,
                    r.roadWidth,
                    r.floors,
                    r.bedrooms,
                    r.bathrooms,
                    r.direction,
                    r.description,
                    r.length,
                    r.width,
                    r.postedDate,
                    images = r.images.Select(img => new {
                        img.imageId,
                        img.imageUrl
                    }).ToList()
                }).ToListAsync();

                return Ok(ApiResult.Success<object>(data, "Danh sách bất động sản", 200));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Error<object>($"Không thể lấy danh sách. Lỗi: {ex.Message}", 400));
            }
        }

        [Authorize]
        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] RealEstateSearchRequest request)
        {
            try
            {
                if (request.pageIndex < 1) request.pageIndex = 1;
                if (request.pageSize < 1) request.pageSize = 10;

                var query = _context.RealEstates.AsQueryable();

                // Tìm theo keyword
                if (!string.IsNullOrWhiteSpace(request.search))
                {
                    var keyword = request.search.ToLower();
                    query = query.Where(x =>
                        x.name.ToLower().Contains(keyword) ||
                        x.price.ToString().Contains(keyword) ||
                        x.province.ToLower().Contains(keyword) ||
                        x.district.ToLower().Contains(keyword) ||
                        x.ward.ToLower().Contains(keyword) ||
                        x.street.ToLower().Contains(keyword) ||
                        x.category.ToLower().Contains(keyword));
                }

                if (request.filter != null)
                {
                    var f = request.filter;
                    if (f.minPrice.HasValue)
                        query = query.Where(x => x.price >= f.minPrice);
                    if (f.maxPrice.HasValue)
                        query = query.Where(x => x.price <= f.maxPrice);
                    if (f.minArea.HasValue)
                        query = query.Where(x => x.area >= f.minArea);
                    if (f.maxArea.HasValue)
                        query = query.Where(x => x.area <= f.maxArea);
                    if (!string.IsNullOrWhiteSpace(f.province))
                        query = query.Where(x => x.province.Contains(f.province));
                    if (!string.IsNullOrWhiteSpace(f.district))
                        query = query.Where(x => x.district.Contains(f.district));
                    if (!string.IsNullOrWhiteSpace(f.ward))
                        query = query.Where(x => x.ward.Contains(f.ward));
                    if (!string.IsNullOrWhiteSpace(f.street))
                        query = query.Where(x => x.street.Contains(f.street));
                    if (!string.IsNullOrWhiteSpace(f.category))
                        query = query.Where(x => x.category.Contains(f.category));
                    if (!string.IsNullOrWhiteSpace(f.status))
                        query = query.Where(x => x.status.Contains(f.status));
                }

                // Tổng số kết quả
                var total = await query.CountAsync();

                // Lấy dữ liệu phân trang và include ảnh
                //var paging = request.paging ?? new PagingRequest();
                //var data = await query
                //    .Skip((request.pageIndex - 1) * request.pageSize)
                //    .Take(request.pageSize)
                //    .Include(x => x.images)
                //    .ToListAsync();

                var data = await query
                    .OrderByDescending(r => r.postedDate)
                    .Skip((request.pageIndex - 1) * request.pageSize)
                    .Take(request.pageSize)
                    .Include(x => x.images)
                    .Select(x => new
                    {
                        x.realEstateId,
                        x.name,
                        x.price,
                        x.province,
                        x.district,
                        x.ward,
                        x.street,
                        x.category,
                        x.status,
                        x.area,
                        x.roadWidth,
                        x.floors,
                        x.bedrooms,
                        x.bathrooms,
                        x.direction,
                        x.description,
                        x.length,
                        x.width,
                        x.postedDate,
                        images = x.images.Select(img => new
                        {
                            img.imageId,
                            img.imageUrl
                        }).ToList()
                    })
                    .ToListAsync();

                var res = new
                {
                    success = true,
                    items = data,
                    pageIndex = request.pageIndex,
                    totalRecords = total,
                    message = "Kết quả tìm kiếm",
                    code = 200
                };

                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Error<object>($"Không thể lấy danh sách. Lỗi: {ex.Message}", 400));
            }
        }

        [Authorize]
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestRealEstates(int count = 10)
        {
            try
            {
                var latestEstates = await _context.RealEstates
                    .OrderByDescending(r => r.postedDate)
                    .Take(count)
                    .Select(r => new
                    {
                        r.realEstateId,
                        r.name,
                        r.price,
                        r.province,
                        r.district,
                        r.ward,
                        r.street,
                        r.category,
                        r.status,
                        r.area,
                        r.roadWidth,
                        r.floors,
                        r.bedrooms,
                        r.bathrooms,
                        r.direction,
                        r.description,
                        r.length,
                        r.width,
                        r.postedDate,
                        images = r.images.Select(i => new
                        {
                            i.imageId,
                            i.imageUrl
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(ApiResult.Success<object>(latestEstates, "Danh sách BĐS mới nhất", 200));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Error<object>($"Không thể lấy danh sách. Lỗi: {ex.Message}", 400));
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : throw new UnauthorizedAccessException("User ID not found");
        }
        //[Authorize]
        //[HttpGet("search")]
        //public async Task<IActionResult> SearchRealEstates(
        //[FromQuery] string? keyword,
        //[FromQuery] string? province,
        //[FromQuery] string? district,
        //[FromQuery] string? ward,
        //[FromQuery] string? category,
        //[FromQuery] string? status,
        //[FromQuery] float? minPrice,
        //[FromQuery] float? maxPrice,
        //[FromQuery] float? minArea,
        //[FromQuery] float? maxArea,
        //[FromQuery] string? direction,
        //[FromQuery] int pageIndex = 1,
        //[FromQuery] int pageSize = 10)
        //{
        //    var query = _context.RealEstates.Include(r => r.Images).AsQueryable();

        //    if (!string.IsNullOrWhiteSpace(keyword))
        //        query = query.Where(r => r.Title.ToLower().Contains(keyword.ToLower()));

        //    if (!string.IsNullOrWhiteSpace(province))
        //        query = query.Where(r => r.Province == province);

        //    if (!string.IsNullOrWhiteSpace(district))
        //        query = query.Where(r => r.District == district);

        //    if (!string.IsNullOrWhiteSpace(ward))
        //        query = query.Where(r => r.Ward == ward);

        //    if (!string.IsNullOrWhiteSpace(category))
        //        query = query.Where(r => r.Category == category);

        //    if (!string.IsNullOrWhiteSpace(status))
        //        query = query.Where(r => r.Status == status);

        //    if (!string.IsNullOrWhiteSpace(direction))
        //        query = query.Where(r => r.Direction == direction);

        //    if (minPrice.HasValue)
        //        query = query.Where(r => r.Price >= minPrice);

        //    if (maxPrice.HasValue)
        //        query = query.Where(r => r.Price <= maxPrice);

        //    if (minArea.HasValue)
        //        query = query.Where(r => r.Area >= minArea);

        //    if (maxArea.HasValue)
        //        query = query.Where(r => r.Area <= maxArea);

        //    var totalRecords = await query.CountAsync();

        //    var items = await query
        //        .OrderByDescending(r => r.CreatedAt)
        //        .Skip((pageIndex - 1) * pageSize)
        //        .Take(pageSize)
        //        .Select(r => new
        //        {
        //            r.RealEstateId,
        //            r.Title,
        //            r.Price,
        //            r.Province,
        //            r.District,
        //            r.Ward,
        //            r.Street,
        //            r.Category,
        //            r.Status,
        //            r.Area,
        //            r.Direction,
        //            r.CreatedAt,
        //            Thumbnail = r.Images.Select(i => i.ImageUrl).FirstOrDefault()
        //        })
        //        .ToListAsync();

        //    var result = new
        //    {
        //        success = true,
        //        message = "Kết quả tìm kiếm",
        //        code = 200,
        //        pageIndex,
        //        pageSize,
        //        totalRecords,
        //        items
        //    };

        //    return Ok(result);
        //}

        //[HttpGet("search")]
        //public async Task<IActionResult> SearchRealEstates(
        //[FromQuery] int? minPrice,
        //[FromQuery] int? maxPrice,
        //[FromQuery] string? address,
        //[FromQuery] string? type,
        //[FromQuery] int pageIndex = 1,
        //[FromQuery] int pageSize = 10)
        //{
        //    try
        //    {
        //        var query = _context.RealEstates
        //            .Include(r => r.Images)
        //            .AsQueryable();

        //        if (minPrice.HasValue)
        //        {
        //            query = query.Where(r => r.Price >= minPrice.Value);
        //        }

        //        if (maxPrice.HasValue)
        //        {
        //            query = query.Where(r => r.Price <= maxPrice.Value);
        //        }

        //        if (!string.IsNullOrWhiteSpace(address))
        //        {
        //            query = query.Where(r =>
        //                (r.City ?? "").ToLower().Contains(address.ToLower()) ||
        //                (r.District ?? "").ToLower().Contains(address.ToLower()) ||
        //                (r.Ward ?? "").ToLower().Contains(address.ToLower()) ||
        //                (r.Street ?? "").ToLower().Contains(address.ToLower()));
        //        }

        //        if (!string.IsNullOrWhiteSpace(type))
        //        {
        //            query = query.Where(r => (r.Type ?? "").ToLower().Contains(type.ToLower()));
        //        }

        //        var totalRecords = await query.CountAsync();

        //        var items = await query
        //            .OrderByDescending(r => r.CreatedAt)
        //            .Skip((pageIndex - 1) * pageSize)
        //            .Take(pageSize)
        //            .Select(r => new
        //            {
        //                r.RealEstateId,
        //                r.Name,
        //                r.Price,
        //                r.City,
        //                r.District,
        //                r.Ward,
        //                r.Street,
        //                r.Type,
        //                r.Status,
        //                r.Area,
        //                r.Direction,
        //                r.CreatedAt,
        //                Images = r.Images.Select(img => img.ImageUrl).ToList()
        //            })
        //            .ToListAsync();

        //        return Ok(new
        //        {
        //            success = true,
        //            message = "Kết quả tìm kiếm",
        //            code = 200,
        //            pageIndex,
        //            totalRecords,
        //            items
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new
        //        {
        //            success = false,
        //            message = $"Tìm kiếm thất bại: {ex.Message}",
        //            code = 400
        //        });
        //    }
        //}

        //[HttpGet("search")]
        //public async Task<IActionResult> SearchRealEstates(
        //[FromQuery] string? search,
        //[FromQuery] int pageIndex = 1,
        //[FromQuery] int pageSize = 10)
        //{
        //    try
        //    {
        //        var query = _context.RealEstates
        //            .Include(r => r.Images)
        //            .AsQueryable();

        //        if (!string.IsNullOrWhiteSpace(search))
        //        {
        //            var keyword = search.ToLower();

        //            // Tìm theo địa chỉ, phân loại, tên, và chuyển đổi chuỗi thành số để tìm theo giá
        //            query = query.Where(r =>
        //                (r.City ?? "").ToLower().Contains(keyword) ||
        //                (r.District ?? "").ToLower().Contains(keyword) ||
        //                (r.Ward ?? "").ToLower().Contains(keyword) ||
        //                (r.Street ?? "").ToLower().Contains(keyword) ||
        //                (r.Type ?? "").ToLower().Contains(keyword) ||
        //                (r.Name ?? "").ToLower().Contains(keyword) ||
        //                (Int32.TryParse(keyword, out int price) && r.Price >= price)
        //            );
        //        }

        //        var totalRecords = await query.CountAsync();

        //        var items = await query
        //            .OrderByDescending(r => r.CreatedAt)
        //            .Skip((pageIndex - 1) * pageSize)
        //            .Take(pageSize)
        //            .Select(r => new
        //            {
        //                r.RealEstateId,
        //                r.Name,
        //                r.Price,
        //                r.City,
        //                r.District,
        //                r.Ward,
        //                r.Street,
        //                r.Type,
        //                r.Status,
        //                r.Area,
        //                r.Direction,
        //                r.CreatedAt,
        //                Images = r.Images.Select(img => img.ImageUrl).ToList()
        //            })
        //            .ToListAsync();

        //        return Ok(new
        //        {
        //            success = true,
        //            message = "Kết quả tìm kiếm",
        //            code = 200,
        //            pageIndex,
        //            totalRecords,
        //            items
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new
        //        {
        //            success = false,
        //            message = $"Tìm kiếm thất bại: {ex.Message}",
        //            code = 400
        //        });
        //    }
        //}
    }
}

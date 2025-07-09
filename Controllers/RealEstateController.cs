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
using System.Data;

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
                    updatedDate = null,
                    createdBy = currentUserId
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
                    estate.createdBy,
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
                var userRole = int.Parse(User.FindFirst(ClaimTypes.Role)?.Value ?? "0");

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

                // Kiểm tra phân quyền: nếu là Đầu Chủ thì chỉ được sửa BĐS của mình
                if (userRole == 2)
                {
                    if (estate.createdBy.HasValue && estate.createdBy.Value != currentUserId)
                        return BadRequest(ApiResult.Error<object>("Tài khoản của bạn không được phép sửa bất động sản", 403));
                }   

                if (userRole == 3)
                {
                    return BadRequest(ApiResult.Error<object>("Tài khoản của bạn không được phép sửa bất động sản", 403));
                }

                if (estate.createdBy == null)
                {
                    estate.createdBy = currentUserId;
                }

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
                    createdBy = estate.createdBy,
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
                var currentUserId = GetCurrentUserId();
                var userRole = int.Parse(User.FindFirst(ClaimTypes.Role)?.Value ?? "0");

                var estate = await _context.RealEstates.FindAsync(id);
                if (estate == null)
                {
                    return NotFound(ApiResult.Error<object>("Bất động sản không tồn tại", 404));
                }

                // Kiểm tra phân quyền: nếu là Đầu Chủ thì chỉ được sửa BĐS của mình
                if (userRole == 2)
                {
                    if (estate.createdBy.HasValue && estate.createdBy.Value != currentUserId)
                        return BadRequest(ApiResult.Error<object>("Tài khoản của bạn không được phép sửa bất động sản", 403));
                }

                if (userRole == 3)
                {
                    return BadRequest(ApiResult.Error<object>("Tài khoản của bạn không được phép sửa bất động sản", 403));
                }

                if (estate.createdBy == null)
                {
                    estate.createdBy = currentUserId;
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
                var currentUserId = GetCurrentUserId();
                var userRole = int.Parse(User.FindFirst(ClaimTypes.Role)?.Value ?? "0");

                var realEstate = await _context.RealEstates
                    .Include(r => r.images)
                    .FirstOrDefaultAsync(r => r.realEstateId == id);

                if (realEstate == null)
                {
                    return NotFound(ApiResult.Error<object>("Bất động sản không tồn tại", 404));
                }

                // Kiểm tra phân quyền: nếu là Đầu Chủ thì chỉ được sửa BĐS của mình
                if (userRole == 2)
                {
                    if (realEstate.createdBy.HasValue && realEstate.createdBy.Value != currentUserId)
                        return BadRequest(ApiResult.Error<object>("Tài khoản của bạn không được phép xóa bất động sản này", 403));
                }

                if (userRole == 3)
                {
                    return BadRequest(ApiResult.Error<object>("Tài khoản của bạn không được phép xóa bất động sản", 403));
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

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRealEstateById(int id)
        {
            var showNews = _context.AppConfigs
                .FirstOrDefault(x => x.key == "NewsScreen")?.value;

            var show = showNews == "true";

            var userAgent = Request.Headers["User-Agent"].ToString();
            var isAndroid = userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase);

            // Lấy config Android
            var showNewsAndr = _context.AppConfigs
                .FirstOrDefault(x => x.key == "NewsScreenAndr")?.value;

            var showAndr = showNewsAndr == "true";

            if (isAndroid)
            {
                if (!showAndr && (!HttpContext.User.Identity?.IsAuthenticated ?? true))
                {
                    return Unauthorized(ApiResult.Error<object>("Không có quyền truy cập", 401));
                }
            }
            else
            {
                // Nếu không phải public mà không có token -> từ chối
                //if (!show && !isAndroid && (!HttpContext.User.Identity?.IsAuthenticated ?? true))
                if (!show && (!HttpContext.User.Identity?.IsAuthenticated ?? true))
                {
                    return Unauthorized(ApiResult.Error<object>("Không có quyền truy cập", 401));
                }
            }

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
                    realEstate.highlightedDate,
                    realEstate.createdBy,
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

        //[Authorize]
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var showNews = _context.AppConfigs
                .FirstOrDefault(x => x.key == "NewsScreen")?.value;

            var show = showNews == "true";

            var userAgent = Request.Headers["User-Agent"].ToString();
            var isAndroid = userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase);

            // Lấy config Android
            var showNewsAndr = _context.AppConfigs
                .FirstOrDefault(x => x.key == "NewsScreenAndr")?.value;

            var showAndr = showNewsAndr == "true";

            if (isAndroid)
            {
                if (!showAndr && (!HttpContext.User.Identity?.IsAuthenticated ?? true))
                {
                    return Unauthorized(ApiResult.Error<object>("Không có quyền truy cập", 401));
                }
            }
            else
            {
                // Nếu không phải public mà không có token -> từ chối
                //if (!show && !isAndroid && (!HttpContext.User.Identity?.IsAuthenticated ?? true))
                if (!show && (!HttpContext.User.Identity?.IsAuthenticated ?? true))
                {
                    return Unauthorized(ApiResult.Error<object>("Không có quyền truy cập", 401));
                }
            }

            try
            {
                var data = await _context.RealEstates
                .Include(r => r.images)
                .OrderByDescending(r => r.highlightedDate ?? r.postedDate)
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
                    r.createdBy,
                    r.highlightedDate,
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

        //[Authorize]
        [AllowAnonymous]
        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] RealEstateSearchRequest request)
        {
            var showNews = _context.AppConfigs
                .FirstOrDefault(x => x.key == "NewsScreen")?.value;

            var show = showNews == "true";

            var userAgent = Request.Headers["User-Agent"].ToString();
            var isAndroid = userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase);

            // Lấy config Android
            var showNewsAndr = _context.AppConfigs
                .FirstOrDefault(x => x.key == "NewsScreenAndr")?.value;

            var showAndr = showNewsAndr == "true";

            if (isAndroid)
            {
                if (!showAndr && (!HttpContext.User.Identity?.IsAuthenticated ?? true))
                {
                    return Unauthorized(ApiResult.Error<object>("Không có quyền truy cập", 401));
                }
            }
            else
            {
                // Nếu không phải public mà không có token -> từ chối
                //if (!show && !isAndroid && (!HttpContext.User.Identity?.IsAuthenticated ?? true))
                if (!show && (!HttpContext.User.Identity?.IsAuthenticated ?? true))
                {
                    return Unauthorized(ApiResult.Error<object>("Không có quyền truy cập", 401));
                }
            }

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
                    .OrderByDescending(r => r.highlightedDate ?? r.postedDate)
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
                        x.createdBy,
                        x.highlightedDate,
                        //isPush = x.postedDate.AddHours(48) <= DateTime.UtcNow,
                        //isPush = true,
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

        [Authorize(Roles = "2")]
        [HttpPost("search-by-owner")]
        public async Task<IActionResult> SearchByOwner([FromBody] RealEstateSearchRequest request)
        {
            //var showNews = _context.AppConfigs
            //    .FirstOrDefault(x => x.key == "NewsScreen")?.value;

            //var show = showNews == "true";

            //var userAgent = Request.Headers["User-Agent"].ToString();
            //var isAndroid = userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase);

            // Nếu không phải public mà không có token -> từ chối
            //if (!show && !isAndroid && (!HttpContext.User.Identity?.IsAuthenticated ?? true))
            //{
            //    return Unauthorized(ApiResult.Error<object>("Không có quyền truy cập", 401));
            //}

            try
            {
                var currentUserId = GetCurrentUserId();
                if (request.pageIndex < 1) request.pageIndex = 1;
                if (request.pageSize < 1) request.pageSize = 10;

                var query = _context.RealEstates.Where(x => x.createdBy == currentUserId).AsQueryable();

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
                    .OrderByDescending(r => r.highlightedDate ?? r.postedDate)
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
                        x.createdBy,
                        x.highlightedDate,
                        //isPush = x.postedDate.AddHours(48) <= DateTime.UtcNow,
                        //isPush = true,
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

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestRealEstates(int count = 10)
        {
            var showNews = _context.AppConfigs
                .FirstOrDefault(x => x.key == "NewsScreen")?.value;

            var show = showNews == "true";

            var userAgent = Request.Headers["User-Agent"].ToString();
            var isAndroid = userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase);

            // Lấy config Android
            var showNewsAndr = _context.AppConfigs
                .FirstOrDefault(x => x.key == "NewsScreenAndr")?.value;

            var showAndr = showNewsAndr == "true";

            if (isAndroid)
            {
                if (!showAndr && (!HttpContext.User.Identity?.IsAuthenticated ?? true))
                {
                    return Unauthorized(ApiResult.Error<object>("Không có quyền truy cập", 401));
                }
            }
            else
            {
                // Nếu không phải public mà không có token -> từ chối
                //if (!show && !isAndroid && (!HttpContext.User.Identity?.IsAuthenticated ?? true))
                if (!show && (!HttpContext.User.Identity?.IsAuthenticated ?? true))
                {
                    return Unauthorized(ApiResult.Error<object>("Không có quyền truy cập", 401));
                }
            }

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

        [Authorize]
        [HttpGet("statistics")]
        public async Task<IActionResult> GetRealEstateStatistics()
        {
            var role = int.Parse(User.FindFirst(ClaimTypes.Role)?.Value ?? "0");
            if (role != 1)
                return Unauthorized(ApiResult.Error<object>("Không có quyền truy cập", 401));

            try
            {
                var tongBatDongSan = await _context.RealEstates.CountAsync();
                var trangThai = await _context.RealEstates
                .GroupBy(r => r.status)
                .Select(g => new
                {
                    status = g.Key,
                    count = g.Count()
                })
                .ToListAsync();
                var dauChuList = await _context.Users
                    .Where(u => u.role == 2)
                    .Select(u => new
                    {
                        userId = u.userId,
                        name = u.fullName,
                        // Thống kê bất động sản của từng đầu chủ
                        dangBan = _context.RealEstates.Count(r => r.createdBy == u.userId && r.status == "Đang bán"),
                        landBan = _context.RealEstates.Count(r => r.createdBy == u.userId && r.status == "68 land bán"),
                        chuNhaBan = _context.RealEstates.Count(r => r.createdBy == u.userId && r.status == "Chủ nhà bán"),
                        dungBan = _context.RealEstates.Count(r => r.createdBy == u.userId && r.status == "Dừng bán"),
                        tong = _context.RealEstates.Count(r => r.createdBy == u.userId)
                    })
                .ToListAsync();

                var statistics = new
                {
                    tongBatDongSan,
                    trangThai,
                    dauChu = dauChuList
                };

                //var statistics = new
                //{
                //    tongBatDongSan = await _context.RealEstates.CountAsync(),

                //    trangThai = await _context.RealEstates
                //    .GroupBy(r => r.status)
                //    .Select(g => new
                //    {
                //        status = g.Key,
                //        count = g.Count()
                //    })
                //    .ToListAsync(),

                //    dauChu = await _context.RealEstates
                //    .Where(r => r.createdBy != null)
                //    .GroupBy(r => r.createdBy)
                //    .Select(g => new
                //    {
                //        userId = g.Key,
                //        name = _context.Users.Where(u => u.userId == g.Key).Select(u => u.fullName).FirstOrDefault(),
                //        dangBan = g.Count(r => r.status == "Đang bán"),
                //        landBan = g.Count(r => r.status == "68 land bán"),
                //        chuNhaBan = g.Count(r => r.status == "Chủ nhà bán"),
                //        dungBan = g.Count(r => r.status == "Dừng bán"),
                //        tong = g.Count()
                //    })
                //    .ToListAsync()
                //};

                return Ok(ApiResult.Success(statistics, "Thống kê bất động sản", 200));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Error<object>($"Không thể lấy danh sách. Lỗi: {ex.Message}", 400));
            }
        }

        [Authorize(Roles = "2")]
        [HttpPost("push-noti/{id}")]
        public async Task<IActionResult> PushRealEstate(int id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var estate = await _context.RealEstates.FindAsync(id);
                if (estate == null)
                    return NotFound(ApiResult.Error<object>("Không tìm thấy bất động sản", 404));

                // Kiểm tra quyền: chỉ đầu chủ của BĐS đó được đẩy
                if (estate.createdBy != userId)
                    return Unauthorized(ApiResult.Error<object>("Không có quyền đẩy tin", 403));

                // Kiểm tra thời gian đăng đã đủ 48 giờ chưa
                var hoursSincePosted = (DateTime.UtcNow - estate.postedDate).TotalHours;
                if (hoursSincePosted < 48)
                    return BadRequest(ApiResult.Error<object>("Cần ít nhất 48 giờ sau khi đăng mới được đẩy tin", 400));

                // Đánh dấu nổi bật để hiển thị lên đầu danh sách
                estate.highlightedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Gửi thông báo (ví dụ gửi đến role 3 - đầu khách)
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
                    direction = estate.direction ?? "",
                    description = estate.description ?? "",
                    length = estate.length,
                    width = estate.width,
                    postedDate = estate.postedDate,
                    updatedDate = estate.updatedDate ?? DateTime.UtcNow,
                    images = await _context.RealEstateImages
                        .Where(i => i.realEstateId == id)
                        .Select(i => new RealEstateNotificationDto.ImageDto
                        {
                            imageId = i.imageId,
                            imageUrl = i.imageUrl
                        }).ToListAsync()
                };

                await _notificationService.CreateAndSendNotificationAsync(3, dto, userId);

                return Ok(ApiResult.Success<object>("", "Đẩy tin thành công", 200));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResult.Error<object>($"Lỗi khi đẩy tin: {ex.Message}", 500));
            }
        }

        //[Authorize]
        //[HttpGet("latest")]
        //public async Task<IActionResult> GetLatestRealEstates(int count = 10)
        //{
        //    try
        //    {
        //        var latestEstates = await _context.RealEstates
        //            .OrderByDescending(r => r.postedDate)
        //            .Take(count)
        //            .Select(r => new
        //            {
        //                r.realEstateId,
        //                r.name,
        //                r.price,
        //                r.province,
        //                r.district,
        //                r.ward,
        //                r.street,
        //                r.category,
        //                r.status,
        //                r.area,
        //                r.roadWidth,
        //                r.floors,
        //                r.bedrooms,
        //                r.bathrooms,
        //                r.direction,
        //                r.description,
        //                r.length,
        //                r.width,
        //                r.postedDate,
        //                images = r.images.Select(i => new
        //                {
        //                    i.imageId,
        //                    i.imageUrl
        //                }).ToList()
        //            })
        //            .ToListAsync();

        //        return Ok(ApiResult.Success<object>(latestEstates, "Danh sách BĐS mới nhất", 200));
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

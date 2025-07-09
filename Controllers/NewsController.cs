using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApi.Data;
using System.Xml;
using System.ServiceModel.Syndication;
using FirebaseAdmin.Messaging;
using System.Text.RegularExpressions;

namespace RealEstateApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        public NewsController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpGet("vnexpress")]
        public async Task<IActionResult> GetVnExpressNews([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 10;

            var url = "https://vnexpress.net/rss/bat-dong-san.rss";

            var config = await _context.AppConfigs.FirstOrDefaultAsync(c => c.key == "NewsScreen");
            if (config == null || config.value?.ToLower() != "true")
            {
                var res = new
                {
                    success = true,
                    show = false,
                    message = "Kết quả",
                    code = 200
                };
                return Ok(res);
            }

            var newsList = new List<object>();
            try
            {
                using (var reader = XmlReader.Create(url))
                {
                    var feed = SyndicationFeed.Load(reader);
                    var items = feed.Items.ToList();
                    var totalItems = items.Count;

                    // Phân trang
                    var skip = (pageIndex - 1) * pageSize;
                    if (skip >= totalItems)
                    {
                        return Ok(new
                        {
                            success = true,
                            items = new List<object>(),
                            pageIndex = pageIndex,
                            totalRecords = totalItems,
                            message = "Kết quả news",
                            code = 200
                        });
                    }

                    var pagedItems = items.Skip(skip).Take(pageSize);

                    foreach (var item in pagedItems)
                    {
                        var title = item.Title.Text;
                        var description = item.Summary?.Text ?? "Không có mô tả";
                        var link = item.Links.FirstOrDefault()?.Uri.ToString() ?? "#";
                        var publishDate = item.PublishDate.DateTime;
                        var image = GetImageFromDescription(item.Summary?.Text);

                        // Loại bỏ các thẻ HTML trong description
                        description = System.Text.RegularExpressions.Regex.Replace(description, "<.*?>", string.Empty);

                        newsList.Add(new {
                            title = title,
                            description = description, 
                            url = link,
                            publishDate = publishDate.ToString("dd/MM/yyyy HH:mm"),
                            image = image,
                        });
                    }

                    // Trả về dữ liệu phân trang
                    return Ok(new
                    {
                        success = true,
                        items = newsList,
                        pageIndex = pageIndex,
                        totalRecords = totalItems,
                        message = "Kết quả news",
                        code = 200
                        //TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                    });

                    //var items = feed.Items.Take(10).Select(item => new
                    //{
                    //    title = item.Title,
                    //    link = item.Link,
                    //    description = item.Description,
                    //    publishDate = item.PublishingDate?.ToString("dd/MM/yyyy HH:mm"),
                    //    image = GetImageFromDescription(item.Description)
                    //});
                }
                //return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lấy tin tức: {ex.Message}");
            }
        }

        private string? GetImageFromDescription(string? html)
        {
            if(string.IsNullOrEmpty(html)) return null;

            var match = Regex.Match(html, "<img.*?src=[\"'](.+?)[\"']");
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}

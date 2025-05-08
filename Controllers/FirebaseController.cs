using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApi.Models;

namespace RealEstateApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FirebaseController : Controller
    {
        private readonly FirebaseService? _firebaseService;

        public FirebaseController(FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
        }

        [Authorize]
        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] PushRequest request)
        {
            try
            {
                var result = await _firebaseService!.SendNotification(request.token, request.title, request.body, request.imageUrl);
                return Ok(new { success = true, messageId = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}

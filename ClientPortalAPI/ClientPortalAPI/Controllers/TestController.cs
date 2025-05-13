using Microsoft.AspNetCore.Mvc;

namespace ClientPortalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("API is running!");
    }

}

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Web.Http;

namespace CustomSender.Controllers
{
    [Route("/api/notification")]
    [Authorize]
    public class NotifyApiController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // Create an event with 'event2' and additional data
            await this.NotifyAsync("event2", new { P1 = "p1" });
            return Ok();
        }
    }
}

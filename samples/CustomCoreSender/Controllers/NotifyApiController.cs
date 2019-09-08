using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            await this.NotifyAllAsync("event2", new
            {
                P1 = "p1",
                X = new List<Model> { new Model
            {
                A = new Model2 { B = "XX", C = "XXX" }
            } }
            });
            return Ok();
        }
    }

    public class Model
    {
        public ModelBase A { get; set; }

    }

    public class ModelBase
    {
        public string B { get; set; }

    }

    public class Model2 : ModelBase
    {
        public string C { get; set; }
    }
}

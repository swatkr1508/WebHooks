using System;
using System.Collections.Generic;
using System.Linq;
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
            var eventType = "Job";
            var eventCategory = "Created";

            await this.NotifyAllAsync(eventType, new Model
            {
                EventData = new ModelBase
                {
                    ResourceUrl = Request.Path,
                    resourceId = System.Guid.NewGuid().ToString(),
                    eventDateUtc = DateTime.Now,
                    //EventType = eventType,
                    EventCategory = eventCategory,
                    TenantId = System.Guid.NewGuid().ToString(),
                    TenantType = "Internal"
                }
            }
            );
            return Ok();
        }
    }

    public class Model
    {

        public ModelBase EventData{ get; set; }
    }

}

public class ModelBase
{
    public string ResourceUrl { get; set; }
    public string resourceId { get; set; }
    public DateTime eventDateUtc { get; set; }
    //public string EventType { get; set; }
    public string EventCategory { get; set; }
    public string TenantId { get; set; }
    public string TenantType { get; set; }

}

public class Model2 : ModelBase
{
    public string C { get; set; }
}

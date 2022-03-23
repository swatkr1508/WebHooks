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
            var eventType = "Job.Completed";

            await this.NotifyAllAsync(eventType, new ModelBase
            {
                ResourceUrl = "{host}/api/v1/integration/jobs/{jobId}",
                JobId = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                TenantId = "4fa85f64-5321-2144-b3fc-2c963f66abc3",
                OrganizationId = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                StatusCode = "0x0000000000000000",
                TimeStamp = DateTime.UtcNow

            });
            return Ok();
        }
    }

    public class Model
    {

        public ModelBase EventData { get; set; }
    }

}

public class ModelBase
{

    public string ResourceUrl { get; set; }
    public DateTime TimeStamp { get; set; }
    //public string EventType { get; set; }
    public string JobId { get; set; }
    public string TenantId { get; set; }
    public string OrganizationId { get; set; }
    public string StatusCode { get; set; }


}

public class Model2 : ModelBase
{
    public string C { get; set; }
}

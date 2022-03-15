using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using P112.WebHooks.Pandora;

namespace CustomCoreReceiver.Controllers
{
    public class EntityReceiver : ControllerBase
    {
        [PandoraWebHook(Id = "Job")]
        public IActionResult Event2Fired(JObject data)
        {

            return Ok();
        }

        [PandoraWebHook()]
        public IActionResult TextEntitiesChange(string id, JObject data)
        {

            return Ok();
        }
    }
}

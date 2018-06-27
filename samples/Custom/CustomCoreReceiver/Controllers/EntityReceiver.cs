using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebHooks;
using Newtonsoft.Json.Linq;
using P112.WebHooks.Pandora;

namespace CustomCoreReceiver.Controllers
{
    public class EntityReceiver : ControllerBase
    {
        [PandoraWebHook(Id = "event2")]
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

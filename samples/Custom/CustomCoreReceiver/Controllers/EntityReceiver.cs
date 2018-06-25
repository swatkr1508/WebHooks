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
    public class EntityReceiver : Controller
    {
        [PandoraWebHook(EventName = "LedgersCreated")]
        public IActionResult TextEntitiesChange(string action, JObject data)
        {

            return Ok();
        }

        //[PandoraWebHook()]
        //public IActionResult Fallback(string action, string eventName, JObject data)
        //{

        //    return Ok();
        //}
    }
}

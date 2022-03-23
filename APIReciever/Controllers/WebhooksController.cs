using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebHooks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace APIReciever.Controllers
{
    [ApiController]
    [Route("api/[controller]/incoming/custom")]
    public class WebhooksController : ControllerBase
    {

        [HttpGet]
        public ContentResult Get([FromQuery] string echo)
        {

            if (echo != null)
            {
                return Content(echo);
            }
            else
            {
                Response.StatusCode = 400;

                return Content("The get request should consist echo parameter");
            }
        }

        [HttpPost]
        public async Task<bool> Post()
        {
            var stringBody = await new StreamReader(Request.Body).ReadToEndAsync();
            WebhookBody webhookData = JsonConvert.DeserializeObject<WebhookBody>(stringBody);
            string SignatureHeaderKey = "sha256";
            string SignatureHeaderValueTemplate = SignatureHeaderKey + "={0}";
            string SignatureHeaderName = "X-ISaas-Signature";
            var secret = Encoding.UTF8.GetBytes("fp_LGxPALBqrqLVbq7YzujRn2g8dd*5%F+%A!%Q3");
            Stream ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            JsonSerializer.CreateDefault().Serialize(writer, webhookData);
            await writer.FlushAsync();

            ms.Seek(0, SeekOrigin.Begin);
            using var hasher = new HMACSHA256(secret);

            var sha256 = hasher.ComputeHash(ms);
            var hashValue = string.Format(CultureInfo.InvariantCulture, SignatureHeaderValueTemplate, EncodingUtilities.ToHex(sha256));

            JObject data = JObject.Parse(stringBody);
            /*
            
            string hashValue;
            string hashValue2;
            using (var hasher = new HMACSHA256(secret))
            {
                var serializedBody = data.ToString();
                var encoded = Encoding.UTF8.GetBytes(serializedBody);
                var sha256 = hasher.ComputeHash(encoded);
                hashValue = string.Format(CultureInfo.InvariantCulture, SignatureHeaderValueTemplate, EncodingUtilities.ToHex(sha256));
                
            }

            using (var hasher = new HMACSHA256(secret))
            {
                var encoded = Encoding.UTF8.GetBytes(stringBody);
                var sha256 = hasher.ComputeHash(encoded);
                hashValue2 = string.Format(CultureInfo.InvariantCulture, SignatureHeaderValueTemplate, EncodingUtilities.ToHex(sha256));
            }

            */
            StringValues header;
            Request.Headers.TryGetValue(SignatureHeaderName, out header);
            StringValues userAgent;
            Request.Headers.TryGetValue("User-Agent", out userAgent);

            var headerValue = header.ToString();
            var comp = hashValue == headerValue;
          
            return comp;
        }

    }
}

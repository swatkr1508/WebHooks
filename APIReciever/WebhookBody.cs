using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebHooks;

namespace APIReciever
{
    public class WebhookBody
    {
        public string Id { get; set; }
        public int Attempt { get; set; }
        public NotificationData[] Notifications { get; set; }
    }

    public class NotificationData
    {
        public string EventType { get; set; }
        public string ResourceUrl { get; set; }
        public DateTime TimeStamp { get; set; }
        public string JobId { get; set; }
        public string TenantId { get; set; }
        public string OrganizationId { get; set; }
        public string StatusCode { get; set; }

    }

}

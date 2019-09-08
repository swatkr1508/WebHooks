using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Settings object
    /// </summary>
    public class WebHookSettings
    {
        /// <summary>
        /// Defines the JsonSerializer Settings
        /// </summary>
        public JsonSerializerSettings Settings { get; set; }
    }
}

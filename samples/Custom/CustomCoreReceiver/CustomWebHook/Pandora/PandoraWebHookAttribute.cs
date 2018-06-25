using Microsoft.AspNetCore.WebHooks;
using Microsoft.AspNetCore.WebHooks.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace P112.WebHooks.Pandora
{
    public class PandoraWebHookAttribute : WebHookAttribute
    {
        public PandoraWebHookAttribute()
            : base(PandoraConstants.ReceiverName)
        {

        }
    }
}

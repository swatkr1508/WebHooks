using Microsoft.AspNetCore.WebHooks;
using Microsoft.AspNetCore.WebHooks.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace P112.WebHooks.Pandora
{
    public class PandoraWebHookAttribute : WebHookAttribute, IWebHookBodyTypeMetadata,
        IWebHookEventSelectorMetadata
    {
        private string _eventName;

        public PandoraWebHookAttribute()
            : base(PandoraConstants.ReceiverName)
        {

        }

        public string EventName
        {
            get
            {
                return _eventName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _eventName = value;
            }
        }

        /// <inheritdoc />
        public WebHookBodyType BodyType => WebHookBodyType.Json;
    }
}

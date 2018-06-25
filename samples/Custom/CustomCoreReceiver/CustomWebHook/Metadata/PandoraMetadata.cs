using Microsoft.AspNetCore.WebHooks.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace P112.WebHooks.Metadata
{

    /// <summary>
    /// An <see cref="IWebHookMetadata"/> service containing metadata about the PandoraMetadata receiver.
    /// </summary>
    public class PandoraMetadata : WebHookMetadata, IWebHookPingRequestMetadata,
        IWebHookGetRequestMetadata, IWebHookEventFromBodyMetadata, IWebHookBodyTypeMetadataService
    {
        /// <summary>
        /// Instantiates a new <see cref="PandoraMetadata"/> instance.
        /// </summary>
        public PandoraMetadata()
            : base(PandoraConstants.ReceiverName)
        {
        }

        // IWebHookBodyTypeMetadataService...
        public string PingEventName => PandoraConstants.ReceiverName;

        public string ChallengeQueryParameterName => null;

        public int SecretKeyMinLength => throw new NotImplementedException();

        public int SecretKeyMaxLength => throw new NotImplementedException();

        public bool AllowMissing => true;

        public string BodyPropertyPath => "Action";

        public WebHookBodyType BodyType => WebHookBodyType.Json;
    }


}

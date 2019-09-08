using Microsoft.AspNetCore.WebHooks.Metadata;
using P112.WebHooks.Filters;

namespace P112.WebHooks.Metadata
{

    /// <summary>
    /// An <see cref="IWebHookMetadata"/> service containing metadata about the PandoraMetadata receiver.
    /// </summary>
    public class PandoraMetadata :
        WebHookMetadata,
        IWebHookFilterMetadata,
        IWebHookGetHeadRequestMetadata
    {
        private readonly PandoraVerifySignatureFilter _verifySignatureFilter;

        /// <summary>
        /// Instantiates a new <see cref="PandoraMetadata"/> instance.
        /// </summary>
        public PandoraMetadata(PandoraVerifySignatureFilter verifySignatureFilter)
            : base(PandoraConstants.ReceiverName)
        {
            _verifySignatureFilter = verifySignatureFilter;
        }

        public override WebHookBodyType BodyType => WebHookBodyType.Json;
        
        public bool AllowHeadRequests => false;

        public string ChallengeQueryParameterName => PandoraConstants.Challenge;

        public int SecretKeyMinLength => 0;


        public void AddFilters(WebHookFilterMetadataContext context)
        {
            context.Results.Add(_verifySignatureFilter);
        }
    }


}

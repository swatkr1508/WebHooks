using Microsoft.AspNetCore.WebHooks;

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

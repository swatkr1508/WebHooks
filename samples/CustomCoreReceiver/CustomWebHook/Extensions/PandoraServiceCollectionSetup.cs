using System;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.Extensions.DependencyInjection.Extensions;
using P112.WebHooks.Filters;
using P112.WebHooks.Metadata;

namespace Microsoft.Extensions.DependencyInjection;

internal static class PandoraServiceCollectionSetup
{
    public static void AddPandoraServices(IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        WebHookMetadata.Register<PandoraMetadata>(services);
        services.TryAddSingleton<PandoraVerifySignatureFilter>();
    }
}

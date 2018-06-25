using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebHooks.Filters;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using P112.WebHooks.Metadata;

namespace P112.WebHooks.Internals
{
    internal static class PandoraServiceCollectionSetup
    {
        public static void AddPandoraServices(IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, MvcOptionsSetup>());
            //services.TryAddEnumerable(ServiceDescriptor.Singleton<IWebHookMetadata, PandoraMetadata>());
            services.TryAddSingleton<PandoraVerifySignatureFilter>();

        }

        private class MvcOptionsSetup : IConfigureOptions<MvcOptions>
        {
            /// <inheritdoc />
            public void Configure(MvcOptions options)
            {
                if (options == null)
                {
                    throw new ArgumentNullException(nameof(options));
                }

                options.Filters.AddService<PandoraVerifySignatureFilter>(WebHookSecurityFilter.Order);
            }
        }
    }
}

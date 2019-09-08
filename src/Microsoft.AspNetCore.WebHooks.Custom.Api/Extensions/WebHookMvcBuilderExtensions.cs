using System;
using System.ComponentModel;
using Microsoft.AspNetCore.WebHooks.Internal;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up WebHooks in an <see cref="IMvcBuilder" />.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class WebHookMvcBuilderExtensions
    {
        /// <summary>
        /// Add WebHook configuration and services to the specified <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder" /> to configure.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static IMvcBuilder AddWebHooksApi(this IMvcBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            WebHookServiceCollectionSetup.AddWebHookServicesApi(builder.Services);

            return builder;
        }
    }
}

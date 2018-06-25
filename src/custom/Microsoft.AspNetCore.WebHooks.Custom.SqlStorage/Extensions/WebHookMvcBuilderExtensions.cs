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
        /// <param name="connectionString">The connection string that is used.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static IMvcBuilder AddWebHooksWithSqlStorage(this IMvcBuilder builder, string connectionString)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            WebHookServiceCollectionSetup.AddWebHookServices(builder.Services, connectionString);
            return builder;
        }
    }
}

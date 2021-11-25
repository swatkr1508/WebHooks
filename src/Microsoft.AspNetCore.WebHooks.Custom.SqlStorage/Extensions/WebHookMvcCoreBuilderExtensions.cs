// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using Microsoft.AspNetCore.WebHooks;
using Microsoft.AspNetCore.WebHooks.Custom.SqlStorage.Internal;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for setting up WebHooks in an <see cref="IMvcCoreBuilder" />.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WebHookMvcCoreBuilderExtensions
{
    /// <summary>
    /// Add WebHook configuration and services to the specified <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IMvcCoreBuilder" /> to configure.</param>
    /// <param name="connectionString">The connecion string</param>
    /// <param name="configureOptions">Configure Options</param>
    /// <returns>The <paramref name="builder"/>.</returns>
    public static IMvcCoreBuilder AddWebHooksWithSqlStorage(this IMvcCoreBuilder builder, string connectionString, Action<WebHookSettings> configureOptions)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.AddWebHooks(configureOptions);
        WebHookServiceCollectionSetup.AddWebHookServicesWithSqlBackend(builder.Services, connectionString);
        return builder;
    }
}

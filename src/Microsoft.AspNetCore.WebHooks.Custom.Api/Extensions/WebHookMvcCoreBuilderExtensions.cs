// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using Microsoft.AspNetCore.WebHooks.Internal;

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
    /// <returns>The <paramref name="builder"/>.</returns>
    public static IMvcCoreBuilder AddWebHooksApi(this IMvcCoreBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        WebHookServiceCollectionSetup.AddWebHookServicesApi(builder.Services);

        return builder;
    }
}

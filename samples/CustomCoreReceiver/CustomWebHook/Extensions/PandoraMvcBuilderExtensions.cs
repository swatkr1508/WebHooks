// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for setting up Slack WebHooks in an <see cref="IMvcBuilder" />.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class PandoraMvcBuilderExtensions
{
    /// <summary>
    /// Add Slack WebHook configuration and services to the specified <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IMvcBuilder" /> to configure.</param>
    /// <returns>The <paramref name="builder"/>.</returns>
    public static IMvcBuilder AddPandoraWebHooks(this IMvcBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        PandoraServiceCollectionSetup.AddPandoraServices(builder.Services);
        return builder.AddWebHooks();
    }
}

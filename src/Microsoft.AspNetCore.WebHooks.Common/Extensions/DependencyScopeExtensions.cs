// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Microsoft.AspNetCore.WebHooks;

/// <summary>
/// Extension methods for <see cref="System.IServiceProvider"/> facilitating getting the services used.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class DependencyScopeExtensions
{
    /// <summary>
    /// Gets the <typeparamref name="TService"/> instance registered with the Dependency Injection engine or
    /// null if none are registered.
    /// </summary>
    /// <typeparam name="TService">The type of services to lookup.</typeparam>
    /// <param name="services">The <see cref="IServiceProvider"/> implementation.</param>
    /// <returns>The registered instance or null if none are registered.</returns>
    public static TService GetService<TService>(this IServiceProvider services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        return (TService)services.GetService(typeof(TService));
    }

    /// <summary>
    /// Gets the set of <typeparamref name="TService"/> instances registered with the Dependency Injection engine
    /// or an empty collection if none are registered.
    /// </summary>
    /// <typeparam name="TService">The type of services to lookup.</typeparam>
    /// <param name="services">The <see cref="IServiceProvider"/> implementation.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing the registered instances.</returns>
    public static IEnumerable<TService> GetServices<TService>(this IServiceProvider services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        return services.GetService<IEnumerable<TService>>().Cast<TService>();
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Extension methods for <see cref="IServiceProvider"/> facilitating getting the services used by custom WebHooks APIs.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class DependencyScopeExtensions
    {
        /// <summary>
        /// Gets an <see cref="IWebHookIdValidator"/> implementation registered with the Dependency Injection engine
        /// or a default implementation if none are registered.
        /// </summary>
        /// <param name="services">The <see cref="IServiceProvider"/> implementation.</param>
        /// <returns>The registered <see cref="IWebHookIdValidator"/> instance or a default implementation if none are registered.</returns>
        public static IWebHookIdValidator GetIdValidator(this IServiceProvider services)
        {
            return services.GetService<IWebHookIdValidator>();
        }

        /// <summary>
        /// Gets the set of <see cref="IWebHookRegistrar"/> instances registered with the Dependency Injection engine
        /// or an empty collection if none are registered.
        /// </summary>
        /// <param name="services">The <see cref="IServiceProvider"/> implementation.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing the registered instances.</returns>
        public static IEnumerable<IWebHookRegistrar> GetRegistrars(this IServiceProvider services)
        {
            return services.GetServices<IWebHookRegistrar>();
        }
    }
}

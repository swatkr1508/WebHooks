// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebHooks.Routes;

namespace Microsoft.AspNetCore.WebHooks.Controllers;

/// <summary>
/// The <see cref="WebHookRegistrationsController"/> allows the caller to get the list of filters 
/// with which a WebHook can be registered. This enables a client to provide a user experience
/// indicating which filters can be used when registering a <see cref="WebHook"/>. 
/// </summary>
[Authorize]
[Route("api/webhooks/filters")]
public class WebHookFiltersController : ControllerBase
{
    /// <summary>
    /// Gets all WebHook filters that a user can register with. The filters indicate which WebHook
    /// events that this WebHook will be notified for.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the operation.</returns>
    [HttpGet(Name = WebHookRouteNames.FiltersGetAction)]
    public async Task<IEnumerable<WebHookFilter>> Get()
    {
        IWebHookFilterManager filterManager = HttpContext.RequestServices.GetFilterManager();
        IDictionary<string, WebHookFilter> filters = await filterManager.GetAllWebHookFiltersAsync();
        return filters.Values;
    }
}

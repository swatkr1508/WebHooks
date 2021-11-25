// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AspNetCore.WebHooks.Filters;

/// <summary>
/// Provides basic data model validation based on data annotations.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
internal sealed class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext actionContext)
    {
        if (actionContext == null)
        {
            throw new ArgumentNullException(nameof(actionContext));
        }

        if (!actionContext.ModelState.IsValid)
        {

            actionContext.Result = new BadRequestObjectResult(actionContext.ModelState);
        }
    }
}

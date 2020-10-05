// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.WebHooks
{
    public interface IWebhookPolicyContainer
    {
        IEnumerable<WebHookPolicyItem> GetAllPolicies();
        WebHookPolicyItem GetPolicyFor(WebHook webhook);
        void RemovePolicyFor(WebHook webhook);
    }
}

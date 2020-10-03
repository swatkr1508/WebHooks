// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.WebHooks
{
    public class WebhookPolicyContainer : IWebhookPolicyContainer
    {
        private readonly ConcurrentDictionary<string, WebHookPolicyItem> _policies;

        public WebhookPolicyContainer()
        {
            _policies = new ConcurrentDictionary<string, WebHookPolicyItem>();
        }

        public IEnumerable<WebHookPolicyItem> GetAllPolicies() => _policies.Values.ToList();

        public WebHookPolicyItem GetPolicyFor(WebHook webhook) => _policies.GetOrAdd(webhook.Id, (arg) => new WebHookPolicyItem(arg));

        public void RemovePolicyFor(WebHook webhook) => _policies.TryRemove(webhook.Id, out _);
    }
}

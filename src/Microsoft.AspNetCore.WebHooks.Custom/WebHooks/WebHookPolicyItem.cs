// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Timeout;

namespace Microsoft.AspNetCore.WebHooks
{
    public class WebHookPolicyItem
    {
        private IAsyncPolicy _policy;

        public WebHookPolicyItem(string id)
        {
            Id = id;

            LastUsed = DateTime.UtcNow;
            LastSuccessful = DateTime.UtcNow;

            _policy = CreatePolicy();
        }

        public string Id { get; }
        public DateTime LastUsed { get; private set; }
        public DateTime LastSuccessful { get; private set; }

        public async Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            LastUsed = DateTime.UtcNow;
            await _policy.ExecuteAsync(action, cancellationToken);
            LastSuccessful = DateTime.UtcNow;
        }

        private static IAsyncPolicy CreatePolicy()
        {
            //var timeout = Policy.TimeoutAsync(TimeSpan.FromSeconds(10), Polly.Timeout.TimeoutStrategy.Optimistic);


            var waitAndRetryPolicy = Policy
               .Handle<HttpRequestException>()
               .Or<TimeoutRejectedException>()
               .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(1 * attempt));

            var circuitBreakerPolicy = Policy
               .Handle<Exception>(e => e is TimeoutRejectedException || e is HttpRequestException)
               .CircuitBreakerAsync(
                   exceptionsAllowedBeforeBreaking: 4,
                   durationOfBreak: TimeSpan.FromSeconds(30)
            );
            return Policy.WrapAsync(waitAndRetryPolicy, circuitBreakerPolicy);//.WrapAsync(timeout);
        }
    }
}

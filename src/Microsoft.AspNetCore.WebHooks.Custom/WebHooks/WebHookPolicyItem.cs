// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.WebHooks
{
    public class WebHookPolicyItem
    {
        private static TimeSpan BACKOFF_TIME = TimeSpan.FromMinutes(30);
        private const short BACKOFF_COUNT = 5;

        private object padlock = new object();
        private int failureCount = 0;

        private DateTime? blockedSince;

        public WebHookPolicyItem(string id)
        {
            Id = id;

            LastUsed = DateTime.UtcNow;
            LastSuccessful = DateTime.UtcNow;
        }

        public string Id { get; }
        public DateTime LastUsed { get; private set; }
        public DateTime LastSuccessful { get; private set; }

        public void AcquireUse()
        {
            lock (padlock)
            {
                if (blockedSince.HasValue && DateTimeOffset.UtcNow - blockedSince < BACKOFF_TIME)
                    throw new CircuitBreakerException("Circuitbreaker is open");

                LastUsed = DateTime.Now;
            }
        }

        public void Success()
        {
            lock (padlock)
            {
                failureCount = 0;
                blockedSince = null;
                LastSuccessful = DateTime.Now;
            }
        }

        public void Failure()
        {
            lock (padlock)
            {
                failureCount++;
                if (failureCount > BACKOFF_COUNT)
                    blockedSince = DateTime.UtcNow;
            }
        }
    }

    public class CircuitBreakerException : Exception
    {
        public CircuitBreakerException(string message) : base(message)
        {
        }
    }

}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.WebHooks;

public sealed class WebhookPolicyContainerCleanupService : IHostedService, IDisposable
{
    private readonly IWebhookPolicyContainer _policyContainers;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WebhookPolicyContainerCleanupService> _logger;
    private bool _cleaningPolicy;
    private Timer _timer;

    public WebhookPolicyContainerCleanupService(IWebhookPolicyContainer policyContainers, IServiceProvider serviceProvider, ILogger<WebhookPolicyContainerCleanupService> logger)
    {
        _policyContainers = policyContainers;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("WebhookPolicyContainerCleanupService running.");
        _timer = new Timer(Cleanup, null, TimeSpan.Zero, TimeSpan.FromMinutes(10));
        return Task.CompletedTask;

    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("WebhookPolicyContainerCleanupService is stopping.");
        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    private async void Cleanup(object state)
    {
        if (!_cleaningPolicy)
        {
            _cleaningPolicy = true;
            try
            {
                var policies = _policyContainers.GetAllPolicies();
                var itemsToRemove = new List<WebHookPolicyItem>();
                var itemsToDisable = new List<WebHookPolicyItem>();
                foreach (var item in policies)
                {
                    if (item.LastUsed < DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)))
                    {
                        itemsToRemove.Add(item);
                    }
                    else if (item.LastSuccessful < DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)))
                    {
                        itemsToDisable.Add(item);
                    }
                }

                if (itemsToRemove.Any() || itemsToDisable.Any())
                {
                    using var scope = _serviceProvider.CreateScope();
                    var manager = scope.ServiceProvider.GetRequiredService<IWebHookStore>();
                    var webhooks = await manager.GetAllWebHooksAsync();

                    foreach (var itemToRemove in itemsToRemove)
                    {
                        _policyContainers.RemovePolicyFor(new WebHook { Id = itemToRemove.Id });
                    }

                    foreach (var itemToDisable in itemsToDisable)
                    {
                        var webhook = webhooks.SingleOrDefault(x => x.Id == itemToDisable.Id);
                        if (webhook != null)
                        {
                            _logger.LogInformation($"Pausing webhook registration with id {webhook.Id}");
                            await manager.DisableWebhookAsync(webhook.Id);
                        }
                        _policyContainers.RemovePolicyFor(new WebHook { Id = itemToDisable.Id });
                    }
                }
            }
            finally
            {
                _cleaningPolicy = false;
            }

        }
    }
}

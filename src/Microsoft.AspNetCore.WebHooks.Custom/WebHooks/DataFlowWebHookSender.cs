// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.AspNetCore.WebHooks.Custom.Properties;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.WebHooks;

/// <summary>
/// Provides an implementation of <see cref="IWebHookSender"/> for sending HTTP requests to
/// registered <see cref="WebHook"/> instances using a default <see cref="WebHook"/> wire format
/// and retry mechanism.
/// </summary>
public class DataflowWebHookSender : WebHookSender
{
    private const int DefaultMaxConcurrencyLevel = 8;

    private static readonly Collection<TimeSpan> DefaultRetries = new Collection<TimeSpan> { TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(4) };

    private readonly HttpClient _httpClient;

    private readonly BufferBlock<WebHookWorkItem> _buffer;
    private readonly ActionBlock<WebHookWorkItem>[] _launchers;
    private readonly IWebhookPolicyContainer _policyContainer;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataflowWebHookSender"/> class with a default retry policy.
    /// </summary>
    /// <param name="logger">The current <see cref="ILogger"/>.</param>
    /// <param name="settings">The current <see cref="WebHookSettings"/>.</param>
    public DataflowWebHookSender(IWebhookPolicyContainer policyContainer, ILogger<DataflowWebHookSender> logger, IOptions<WebHookSettings> settings)
        : this(policyContainer, logger, retryDelays: null, options: null, httpClient: null, settings: settings)
    {
    }


    /// <summary>
    /// Initialize a new instance of the <see cref="DataflowWebHookSender"/> with the given retry policy, <paramref name="options"/>,
    /// and <paramref name="httpClient"/>. This constructor is intended for unit testing purposes.
    /// </summary>
    internal DataflowWebHookSender(IWebhookPolicyContainer policyContainer, ILogger<DataflowWebHookSender> logger, IEnumerable<TimeSpan> retryDelays, ExecutionDataflowBlockOptions options, HttpClient httpClient, IOptions<WebHookSettings> settings)
        : base(logger, settings)
    {
        _policyContainer = policyContainer ?? throw new ArgumentNullException(nameof(policyContainer));
        retryDelays = retryDelays ?? DefaultRetries;

        options = options ?? new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = DefaultMaxConcurrencyLevel };

        _httpClient = httpClient ?? new HttpClient();

        // Create the launch processors with the given retry delays
        _launchers = new ActionBlock<WebHookWorkItem>[1 + retryDelays.Count()];

            _buffer = new BufferBlock<WebHookWorkItem>();
            var offset = 0;
            _launchers[offset++] = new ActionBlock<WebHookWorkItem>(async item => await LaunchWebHook(item), options);
            foreach (var delay in retryDelays)
            {
                _launchers[offset++] = new ActionBlock<WebHookWorkItem>(async item => await DelayedLaunchWebHook(item, delay), options);
            }
            _buffer.LinkTo(_launchers[0], new DataflowLinkOptions() { PropagateCompletion = true });

        var message = string.Format(CultureInfo.CurrentCulture, CustomResources.Manager_Started, typeof(DataflowWebHookSender).Name, _launchers.Length);
        Logger.LogInformation(message);
    }

    /// <inheritdoc />
    public override Task SendWebHookWorkItemsAsync(IEnumerable<WebHookWorkItem> workItems)
    {
        if (workItems == null)
        {
            throw new ArgumentNullException(nameof(workItems));
        }

            Logger.LogDebug($"number of items in the launchers: {string.Join(", ", _launchers.Select(x => x.InputCount).ToArray())}");

            foreach (var workItem in workItems)
            {
                _buffer.Post(workItem);
            }

            return Task.CompletedTask;
        }

    /// <summary>
    /// Releases the unmanaged resources and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _disposed = true;
            if (disposing)
            {
                if (_launchers != null)
                {
                    try
                    {
                        // Start shutting down launchers
                        var completionTasks = new Task[_launchers.Length];
                        for (var i = 0; i < _launchers.Length; i++)
                        {
                            var launcher = _launchers[i];
                            launcher.Complete();
                            completionTasks[i] = launcher.Completion;
                        }

                        // Cancel any outstanding HTTP requests
                        if (_httpClient != null)
                        {
                            _httpClient.CancelPendingRequests();
                            _httpClient.Dispose();
                        }

                        // Wait for launchers to complete
                        Task.WaitAll(completionTasks);
                    }
                    catch (Exception ex)
                    {
                        ex = ex.GetBaseException();
                        var message = string.Format(CultureInfo.CurrentCulture, CustomResources.Manager_CompletionFailure, ex.Message);
                        Logger.LogError(message, ex);
                    }
                }
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// If delivery of a WebHook is not successful, i.e. something other than a 2xx or 410 Gone
    /// HTTP status code is received and the request is to be retried, then <see cref="OnWebHookRetry"/>
    /// is called enabling additional post-processing of a retry request.
    /// </summary>
    /// <param name="workItem">The current <see cref="WebHookWorkItem"/>.</param>
    protected virtual Task OnWebHookRetry(WebHookWorkItem workItem) => Task.CompletedTask;


    /// <summary>
    /// If delivery of a WebHook is successful, i.e. a 2xx HTTP status code is received,
    /// then <see cref="OnWebHookSuccess"/> is called enabling additional post-processing.
    /// </summary>
    /// <param name="workItem">The current <see cref="WebHookWorkItem"/>.</param>
    protected virtual Task OnWebHookSuccess(WebHookWorkItem workItem) => Task.CompletedTask;

    /// <summary>
    /// If delivery of a WebHook is not successful, i.e. something other than a 2xx or 410 Gone
    /// HTTP status code is received after having retried the request according to the retry-policy,
    /// then <see cref="OnWebHookFailure"/> is called enabling additional post-processing.
    /// </summary>
    /// <param name="workItem">The current <see cref="WebHookWorkItem"/>.</param>
    protected virtual Task OnWebHookFailure(WebHookWorkItem workItem) => Task.CompletedTask;

    /// <summary>
    /// If delivery of a WebHook results in a 410 Gone HTTP status code, then <see cref="OnWebHookGone"/>
    /// is called enabling additional post-processing.
    /// </summary>
    /// <param name="workItem">The current <see cref="WebHookWorkItem"/>.</param>
    protected virtual Task OnWebHookGone(WebHookWorkItem workItem) => Task.CompletedTask;


    private async Task DelayedLaunchWebHook(WebHookWorkItem item, TimeSpan delay)
    {
        await Task.Delay(delay);
        await LaunchWebHook(item);
    }

    /// <summary>
    /// Launch a <see cref="WebHook"/>.
    /// </summary>
    /// <remarks>We don't let exceptions propagate out from this method as it is used by the launchers
    /// and if they see an exception they shut down.</remarks>
    private async Task LaunchWebHook(WebHookWorkItem workItem)
    {
        var policy = _policyContainer.GetPolicyFor(workItem.WebHook);
        try
        {
            policy.AcquireUse();

            // Setting up and send WebHook request
            using var request = await CreateWebHookRequestAsync(workItem);
            using var response = await _httpClient.SendAsync(request);

        var message = string.Format(CultureInfo.CurrentCulture, CustomResources.Manager_Result, workItem.WebHook.Id, response.StatusCode, workItem.Offset);
        Logger.LogInformation(message);

            if (response.IsSuccessStatusCode)
            {
                policy.Success();
                // If we get a successful response then we are done.
                await OnWebHookSuccess(workItem);
                return;
            }
            else if (response.StatusCode == HttpStatusCode.Gone)
            {
                // If we get a 410 Gone then we are also done.
                await OnWebHookGone(workItem);
                return;
            }
        }
        catch (CircuitBreakerException)
        {
            var message = string.Format(CultureInfo.CurrentCulture, CustomResources.Manager_GivingUp, workItem.WebHook.Id, workItem.Offset);
            Logger.LogInformation(message);
            await OnWebHookFailure(workItem);
            return;
        }
        catch (Exception ex)
        {
            var message = string.Format(CultureInfo.CurrentCulture, CustomResources.Manager_WebHookFailure, workItem.Offset, workItem.WebHook.Id, ex.Message);
            Logger.LogInformation(message, ex);
        }

        try
        {
            // See if we should retry the request with delay or give up
            workItem.Offset++;
            policy.Failure();
            if (workItem.Offset < _launchers.Length)
            {
                // If we are to retry then we submit the request again after a delay.
                await OnWebHookRetry(workItem);
                _launchers[workItem.Offset].Post(workItem);
            }
            else
            {
                var message = string.Format(CultureInfo.CurrentCulture, CustomResources.Manager_GivingUp, workItem.WebHook.Id, workItem.Offset);
                Logger.LogInformation(message);
                await OnWebHookFailure(workItem);
            }
        }
        catch (Exception ex)
        {
            var message = string.Format(CultureInfo.CurrentCulture, CustomResources.Manager_WebHookFailure, workItem.Offset, workItem.WebHook.Id, ex.Message);
            Logger.LogInformation(message, ex);
        }
    }
}

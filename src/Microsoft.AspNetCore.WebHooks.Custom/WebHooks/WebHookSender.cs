// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebHooks.Custom.Properties;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Provides a base implementation of <see cref="IWebHookSender"/> that defines the default format
    /// for HTTP requests sent as WebHooks.
    /// </summary>
    public abstract class WebHookSender : IWebHookSender, IDisposable
    {
        internal const string SignatureHeaderKey = "sha256";
        internal const string SignatureHeaderValueTemplate = SignatureHeaderKey + "={0}";
        internal const string SignatureHeaderName = "ms-signature";

        private const string BodyIdKey = "Id";
        private const string BodyAttemptKey = "Attempt";
        private const string BodyPropertiesKey = "Properties";
        private const string BodyNotificationsKey = "Notifications";

        private readonly ILogger _logger;
        private readonly WebHookSettings _settings;
        private readonly JsonSerializer _serializer;
        private bool _disposed;

        private static MediaTypeHeaderValue ApplicationJson = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        /// <summary>
        /// Initializes a new instance of the <see cref="WebHookSender"/> class.
        /// </summary>
        protected WebHookSender(ILogger<WebHookSender> logger, IOptions<WebHookSettings> settings)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings.Value;

            _serializer = _settings.Settings != null ? JsonSerializer.Create(_settings.Settings) : JsonSerializer.CreateDefault();
            _serializer.Converters.Add(new NotificationDictionarySerializer());
        }

        /// <summary>
        /// Gets the current <see cref="ILogger"/> instance.
        /// </summary>
        protected ILogger Logger => _logger;

        /// <inheritdoc />
        public abstract Task SendWebHookWorkItemsAsync(IEnumerable<WebHookWorkItem> workItems);

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                if (disposing)
                {
                    // Dispose any resources
                }
            }
        }

        /// <summary>
        /// Creates an <see cref="HttpRequestMessage"/> containing the headers and body given a <paramref name="workItem"/>.
        /// </summary>
        /// <param name="workItem">A <see cref="WebHookWorkItem"/> representing the <see cref="WebHook"/> to be sent.</param>
        /// <returns>A filled in <see cref="HttpRequestMessage"/>.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Request is disposed by caller.")]
        protected virtual async Task<HttpRequestMessage> CreateWebHookRequest(WebHookWorkItem workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            var hook = workItem.WebHook;

            // Create WebHook request
            var request = new HttpRequestMessage(HttpMethod.Post, hook.WebHookUri);

            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            CreateWebHookRequestBody(workItem, writer);
            await writer.FlushAsync();
            ms.Seek(0, SeekOrigin.Begin);
            SignWebHookRequest(workItem, request, ms);

            // Add extra request or entity headers
            foreach (var kvp in hook.Headers)
            {
                if (!request.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value))
                {
                    if (!request.Content.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value))
                    {
                        var message = string.Format(CultureInfo.CurrentCulture, CustomResources.Manager_InvalidHeader, kvp.Key, hook.Id);
                        _logger.LogError(message);
                    }
                }
            }

            return request;
        }

        /// <summary>
        /// Creates a <see cref="JObject"/> used as the <see cref="HttpRequestMessage"/> entity body for a <see cref="WebHook"/>.
        /// </summary>
        /// <param name="workItem">The <see cref="WebHookWorkItem"/> representing the data to be sent.</param>
        /// <returns>An initialized <see cref="JObject"/>.</returns>
        protected virtual void CreateWebHookRequestBody(WebHookWorkItem workItem, StreamWriter writer)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            // Set notifications
            var webhookBody = new WebhookBody
            {
                Id = workItem.Id,
                Attempt = workItem.Offset + 1,
            };
            var properties = workItem.WebHook.Properties;
            if (properties != null)
            {
                webhookBody.Properties = new Dictionary<string, object>(properties);
            }
            webhookBody.Notifications = workItem.Notifications.ToArray();
            _serializer.Serialize(writer, webhookBody);
        }

        /// <summary>
        /// Adds a SHA 256 signature to the <paramref name="body"/> and adds it to the <paramref name="request"/> as an
        /// HTTP header to the <see cref="HttpRequestMessage"/> along with the entity body.
        /// </summary>
        /// <param name="workItem">The current <see cref="WebHookWorkItem"/>.</param>
        /// <param name="request">The request to add the signature to.</param>
        /// <param name="body">The body to sign and add to the request.</param>
        protected virtual void SignWebHookRequest(WebHookWorkItem workItem, HttpRequestMessage request, Stream body)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }
            if (workItem.WebHook == null)
            {
                var message = string.Format(CultureInfo.CurrentCulture, CustomResources.Sender_BadWorkItem, GetType().Name, "WebHook");
                throw new ArgumentException(message, "workItem");
            }
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            var secret = Encoding.UTF8.GetBytes(workItem.WebHook.Secret);
            using var hasher = new HMACSHA256(secret);

            var sha256 = hasher.ComputeHash(body);
            var headerValue = string.Format(CultureInfo.InvariantCulture, SignatureHeaderValueTemplate, EncodingUtilities.ToHex(sha256));
            request.Headers.Add(SignatureHeaderName, headerValue);

            body.Seek(0, SeekOrigin.Begin);

            var content = new StreamContent(body);
            content.Headers.ContentType = ApplicationJson;
            request.Content = content;
        }
    }

    internal class NotificationDictionarySerializer : JsonConverter
    {
        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(NotificationDictionary);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            NotificationDictionary dictionary = (NotificationDictionary)value;
            writer.WriteStartObject();
            foreach (var key in dictionary)
            {
                writer.WritePropertyName(key.Key);
                serializer.Serialize(writer, key.Value);
            }
            writer.WriteEndObject();
        }
    }

    internal class WebhookBody
    {
        public string Id { get; set; }
        public int Attempt { get; set; }
        public Dictionary<string, object> Properties { get; set; }
        public NotificationDictionary[] Notifications { get; set; }
    }
}

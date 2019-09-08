using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.AspNetCore.WebHooks.Custom.Tests
{
    [TestClass]
    public class PollyWebHookSenderTests
    {
        private const string Secret = "12345678123456781234567812345678";

        [TestMethod]
        public async Task SendWebhookWorkItem_Test()
        {
            var logger = new DummyLogger<PollyWebHookSender>();
            var httpClient = new HttpClient(new NormalWebhookHandler());
            var target = new WebhookSenderProxy(httpClient, logger, new OptionsWrapper<WebHookSettings>(new WebHookSettings()));

            await target.SendWebHookWorkItemsAsync(new List<WebHookWorkItem>
            {
                new WebHookWorkItem(new WebHook { WebHookUri = new System.Uri("http://local"), Secret = Secret }, new List<NotificationDictionary>()
                {
                    new NotificationDictionary("test", new { Bla = "Test" })
                })
            });

            Assert.AreEqual(1, target.Attempts);
            Assert.IsTrue(target.Success);
            Assert.IsFalse(target.Failed);
        }

        [TestMethod]
        public async Task SendWebhookWorkItem_FailingendPoint_Test()
        {
            var logger = new DummyLogger<PollyWebHookSender>();
            var httpClient = new HttpClient(new ExceptionWebhookHandler());
            var target = new WebhookSenderProxy(httpClient, logger, new OptionsWrapper<WebHookSettings>(new WebHookSettings()));

            await target.SendWebHookWorkItemsAsync(new List<WebHookWorkItem>
            {
                new WebHookWorkItem(new WebHook { WebHookUri = new System.Uri("http://local"), Secret = Secret }, new List<NotificationDictionary>()
                {
                    new NotificationDictionary("test", new { Bla = "Test" })
                })
            });

            Assert.AreEqual(4, target.Attempts);
            Assert.IsTrue(target.Failed);
            Assert.IsFalse(target.Success);
        }

        [TestMethod]
        public async Task SendWebhookWorkItem_CircuitBreaker_Test()
        {
            var logger = new DummyLogger<PollyWebHookSender>();
            var httpClient = new HttpClient(new ExceptionWebhookHandler());
            var target = new WebhookSenderProxy(httpClient, logger, new OptionsWrapper<WebHookSettings>(new WebHookSettings()));

            await target.SendWebHookWorkItemsAsync(new List<WebHookWorkItem>
            {
                new WebHookWorkItem(new WebHook { WebHookUri = new System.Uri("http://local"), Secret = Secret }, new List<NotificationDictionary>()
                {
                    new NotificationDictionary("test", new { Bla = "Test" }),
                })
            });

            await target.SendWebHookWorkItemsAsync(new List<WebHookWorkItem>
            {
                new WebHookWorkItem(new WebHook { WebHookUri = new System.Uri("http://local"), Secret = Secret }, new List<NotificationDictionary>()
                {
                    new NotificationDictionary("test", new { Bla = "Test" }),
                })
            });

            await target.SendWebHookWorkItemsAsync(new List<WebHookWorkItem>
            {
                new WebHookWorkItem(new WebHook { WebHookUri = new System.Uri("http://local"), Secret = Secret }, new List<NotificationDictionary>()
                {
                    new NotificationDictionary("test", new { Bla = "Test" }),
                })
            });

            Assert.AreEqual(4, target.Attempts);
            Assert.IsTrue(target.Failed);
            Assert.IsFalse(target.Success);
        }

        [TestMethod]
        public async Task SendWebhookWorkItem_Timeout_Test()
        {
            var logger = new DummyLogger<PollyWebHookSender>();
            var httpClient = new HttpClient(new TimeoutWebhookHandler());
            var target = new WebhookSenderProxy(httpClient, logger, new OptionsWrapper<WebHookSettings>(new WebHookSettings()));

            await target.SendWebHookWorkItemsAsync(new List<WebHookWorkItem>
            {
                new WebHookWorkItem(new WebHook { WebHookUri = new System.Uri("http://local"), Secret = Secret }, new List<NotificationDictionary>()
                {
                    new NotificationDictionary("test", new { Bla = "Test" }),
                })
            });

            Assert.AreEqual(4, target.Attempts);
            Assert.IsTrue(target.Failed);
            Assert.IsFalse(target.Success);
        }


        private class NormalWebhookHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                return Task.FromResult(response);
            }
        }

        private class ExceptionWebhookHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw new HttpRequestException();
            }
        }

        private class TimeoutWebhookHandler : HttpMessageHandler
        {
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                await Task.Delay(TimeSpan.FromSeconds(20), cancellationToken);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        }

        private class WebhookSenderProxy : PollyWebHookSender
        {
            public WebhookSenderProxy(HttpClient httpClient, ILogger<PollyWebHookSender> logger, IOptions<WebHookSettings> settings)
                : base(httpClient, logger, settings)
            {

            }

            public int Attempts { get; private set; }
            public bool Failed { get; private set; }
            public bool Success { get; private set; }

            protected override Task OnWebHookAttempt(WebHookWorkItem workItem)
            {
                Attempts++;
                return base.OnWebHookAttempt(workItem);
            }

            protected override Task OnWebHookSuccess(WebHookWorkItem workItem)
            {
                Success = true;
                return base.OnWebHookSuccess(workItem);
            }

            protected override Task OnWebHookFailure(WebHookWorkItem workItem)
            {
                Failed = true;
                return base.OnWebHookFailure(workItem);
            }
        }
    }
}

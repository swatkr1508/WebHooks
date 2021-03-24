using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.AspNetCore.WebHooks.Custom.Tests
{
    [TestClass]
    public class WebHookPolicyItemTests
    {
        [TestMethod]
        public void DefaultTest()
        {
            var target = new WebHookPolicyItem("id");
            target.AcquireUse();
            target.Success();

            Assert.AreNotEqual(DateTime.MinValue, target.LastUsed);
            Assert.AreNotEqual(DateTime.MinValue, target.LastSuccessful);
        }

        [TestMethod, ExpectedException(typeof(CircuitBreakerException))]
        public void CircuitBreaker_Test()
        {
            var target = new WebHookPolicyItem("id");
            target.AcquireUse();
            target.Failure();
            target.AcquireUse();
            target.Failure();
            target.AcquireUse();
            target.Failure();
            target.AcquireUse();
            target.Failure();
            target.AcquireUse();
            target.Failure();
            target.AcquireUse();
            target.Failure();

            target.AcquireUse();
            target.Failure();
        }
    }
}

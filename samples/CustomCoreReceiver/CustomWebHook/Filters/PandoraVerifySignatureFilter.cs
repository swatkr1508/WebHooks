using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebHooks.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace P112.WebHooks.Filters;

public sealed class PandoraVerifySignatureFilter : WebHookVerifySignatureFilter, IAsyncResourceFilter
{
    public PandoraVerifySignatureFilter(IConfiguration configuration, IHostEnvironment hostingEnvironment, ILoggerFactory loggerFactory)
        : base(configuration, hostingEnvironment, loggerFactory)
    {
    }

    public override string ReceiverName => "Pandora";

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }
        if (next == null)
        {
            throw new ArgumentNullException(nameof(next));
        }

        var routeData = context.RouteData;
        var request = context.HttpContext.Request;
        if (routeData.TryGetWebHookReceiverName(out var receiverName) &&
            IsApplicable(receiverName) &&
            HttpMethods.IsPost(request.Method))
        {

        }

        await next();
    }
}

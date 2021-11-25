using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.WebHooks;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions for the IApplicationBuilderExtensions
/// </summary>
public static class IApplicationBuilderExtensions
{
    /// <summary>
    /// Start webhook services
    /// </summary>
    /// <param name="app"></param>
    public static void UseWebHooks(this IApplicationBuilder app)
    {
        using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
        {
            var context = serviceScope.ServiceProvider.GetRequiredService<WebHookStoreContext>();
            context.Database.Migrate();
        }
    }
}

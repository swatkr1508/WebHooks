using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CustomCoreSender
{
    public static class CustomDefaults
    {
        public const string AuthenticationScheme = "CustomSchema";
    }


    public static class CustomIdentity
    {
        public static AuthenticationBuilder AddCustom(this AuthenticationBuilder builder)
           => builder.AddCustom(CustomDefaults.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddCustom(this AuthenticationBuilder builder, Action<AppIdentityOptions> configureOptions)
            => builder.AddCustom(CustomDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddCustom(this AuthenticationBuilder builder, string authenticationScheme, Action<AppIdentityOptions> configureOptions)
            => builder.AddCustom(authenticationScheme, displayName: null, configureOptions: configureOptions);

        public static AuthenticationBuilder AddCustom(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<AppIdentityOptions> configureOptions)
        {
            //builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<JwtBearerOptions>, JwtBearerPostConfigureOptions>());

            return builder.AddScheme<AppIdentityOptions, AppIdentityHandler>(authenticationScheme, displayName, configureOptions);
        }
    }
    public class AppIdentityOptions : AuthenticationSchemeOptions
    {

    }

    public class AppIdentityHandler : AuthenticationHandler<AppIdentityOptions>
    {
        public AppIdentityHandler(IOptionsMonitor<AppIdentityOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {

        }


        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var identity = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.NameIdentifier, "Pandora"),
            }, CustomDefaults.AuthenticationScheme);

            var tokenValidatedContext = new TokenValidatedContext(Context, Scheme, Options)
            {
                Principal = new ClaimsPrincipal(identity),
            };
            tokenValidatedContext.Success();
            var r = tokenValidatedContext.Result;
            return Task.FromResult(r);
        }
    }


    public class TokenValidatedContext : ResultContext<AppIdentityOptions>

    {

        public TokenValidatedContext(
            HttpContext context,

            AuthenticationScheme scheme,

            AppIdentityOptions options)

            : base(context, scheme, options) { }


    }
}

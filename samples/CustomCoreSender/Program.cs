using CustomCoreSender;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

const string SwaggerTitle = "CustomCoreSender - API";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers()
            .AddWebHooksApi()
            .AddWebHooksWithSqlStorage("Server=(localdb)\\MSSQLLocalDB;Database=WebHooks_dev;Integrated Security=true;", x =>
            {
                x.Settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                };
            });

builder.Services.AddAuthentication(CustomDefaults.AuthenticationScheme)
    .AddCustom();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = SwaggerTitle, Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app
    .UseRouting()
    .UseAuthentication()
    .UseAuthorization()
    .UseStaticFiles()
    .UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    })
    .UseWebHooks();

app
    .UseSwagger()
    .UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", SwaggerTitle);
    });

app.Run();

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddControllers()
    .AddPandoraWebHooks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app
    .UseRouting()
    .UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });

app.Run();

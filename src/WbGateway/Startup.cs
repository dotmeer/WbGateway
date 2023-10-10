using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Prometheus;
using WbGateway.Implementations;
using WbGateway.Interfaces;

namespace WbGateway;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddControllers()
            .AddControllersAsServices()
            .AddNewtonsoftJson(_ =>
            {
                _.SerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffK";
                _.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                _.SerializerSettings.Converters.Add(new StringEnumConverter());
            });

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Default", Version = "v1" });
            options.CustomSchemaIds(_ => _.FullName);
            options.UseAllOfToExtendReferenceSchemas();
            options.SupportNonNullableReferenceTypes();
        });
        services.AddSwaggerGenNewtonsoftSupport();

        services.AddRouting(options => { options.AppendTrailingSlash = true; });

        services.AddSingleton<IMqttClientFactory, MqttClientFactory>();
        services.AddHostedService<Zigbee2MqttBackgroundJob>();
        services.AddHostedService<MqttToPrometheusBackgroundJob>();
        //services.AddHostedService<TestMqttBackgroundJob>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger(swaggerOptions => { swaggerOptions.RouteTemplate = "/swagger/{documentName}/swagger.json"; });

        app.UseSwaggerUI(swaggerUiOptions =>
        {
            swaggerUiOptions.RoutePrefix = "swagger";
            swaggerUiOptions.SwaggerEndpoint("/swagger/v1/swagger.json", "API");
            swaggerUiOptions.DisplayRequestDuration();
        });

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapMetrics();
        });
    }
}
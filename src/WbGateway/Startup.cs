using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using WbGateway.Application;
using WbGateway.BackgroundServices;
using WbGateway.Infrastructure.Metrics;
using WbGateway.Infrastructure.Mqtt;

namespace WbGateway;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddControllers()
            .AddControllersAsServices()
            .AddJsonOptions(_ =>
            {
                _.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                _.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

        services
            .AddMetrics()
            .AddMqtt()
            .AddApplication();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Default", Version = "v1" });
            options.CustomSchemaIds(_ => _.FullName);
            options.UseAllOfToExtendReferenceSchemas();
            options.SupportNonNullableReferenceTypes();
        });

        services.AddRouting(options => { options.AppendTrailingSlash = true; });

        services.AddHostedService<Zigbee2MqttBackgroundJob>();
        services.AddHostedService<MqttTopicsMetricsBackgroundJob>();
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
            endpoints.AddMetricsPullHost();
        });
    }
}
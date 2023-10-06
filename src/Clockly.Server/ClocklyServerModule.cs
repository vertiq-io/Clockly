using Clockly.Services;

namespace Clockly;

[DependsOn(typeof(ClocklyShellModule))]
[DependsOn(typeof(VertiqBlazorServerDefaultModule))]
[DependsOn(typeof(VertiqBlazorServerSecurityModule))]
[DependsOn(typeof(VertiqSecurityModule))]
[DependsOn(typeof(VertiqHttpTransportServerModule))]
[DependsOn(typeof(VertiqNewtonsoftJsonSerializationModule))]
[DependsOn(typeof(VertiqXpoSchemaUpdateModule))]
public sealed record ClocklyServerModule : ModuleBase
{
    public override void ConfigureServices(IApplication application, IServiceCollection services)
    {
        base.ConfigureServices(application, services);
        services.AddTransient<ICurrentUserService, CurrentUserService>();
    }
}
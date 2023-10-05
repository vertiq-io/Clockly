namespace Clockly;

[DependsOn(typeof(ClocklyShellModule))]
[DependsOn(typeof(VertiqBlazorServerDefaultModule))]
[DependsOn(typeof(VertiqBlazorServerSecurityModule))]
[DependsOn(typeof(VertiqSecurityModule))]
[DependsOn(typeof(VertiqHttpTransportServerModule))]
[DependsOn(typeof(VertiqNewtonsoftJsonSerializationModule))]
public sealed record ClocklyServerModule : ModuleBase
{
    
}
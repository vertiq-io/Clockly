using Clockly.Models;

using DevExpress.Xpo.DB;

namespace Clockly;

[DependsOn(typeof(VertiqMudBlazorTemplateModule))]
public sealed record ClocklyShellModule : ModuleBase
{ 
    static ClocklyShellModule()
    {
        SQLiteConnectionProvider.Register();
    }

    public override void RegisterNavItems(NavItemCollection navItems) => navItems
        .Add(("Home", "/"))
    ;

    public override void ConfigureServices(IApplication application, IServiceCollection services)
    {
        base.ConfigureServices(application, services);
                
        services.AddXpoHive(nameof(ClocklyShellModule), o => o
            .WithAutoCreateOption(DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema)
            .WithConnectionString(SQLiteConnectionProvider.GetConnectionString($"{nameof(Clockly)}.db"))
            .AddTypes(typeof(CTimeEntry))
        );
    }
}
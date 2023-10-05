using Clockly.Models;

using DevExpress.Xpo.DB;

using Vertiq.Icons;

namespace Clockly;

[DependsOn(typeof(VertiqMudBlazorTemplateModule))]
[DependsOn(typeof(VertiqFluxorConventionsModule))]
[DependsOn(typeof(VertiqMaterialDesignIconsIconPackModule))]
public sealed record ClocklyShellModule : ModuleBase
{ 
    static ClocklyShellModule()
    {
        SQLiteConnectionProvider.Register();
    }

    public override void RegisterNavItems(NavItemCollection navItems) => navItems
        .Add(("Home", "/"))
        .Add(("Tracker", "/timetracker", MdiIcons.ClockCheck))
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
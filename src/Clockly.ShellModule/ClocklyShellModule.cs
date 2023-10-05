namespace Clockly;

[DependsOn(typeof(VertiqMudBlazorTemplateModule))]
public sealed record ClocklyShellModule : ModuleBase
{
    public override void RegisterNavItems(NavItemCollection navItems) => navItems
        .Add(("Home", "/"))
    ;
}
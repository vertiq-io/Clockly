namespace Clockly;

public sealed record ClocklyApplication : ApplicationBase
{
    public ClocklyApplication(
        IConfiguration configuration,
        ILogger logger,
        IServiceCollection services,
        string environmentName
    ) : base(configuration, logger, services, environmentName)
    {
        Catalog.AddModule<ClocklyShellModule>();
        MainLayoutType = typeof(Vertiq.Shared.MudBlazorMainLayout);
    }
}

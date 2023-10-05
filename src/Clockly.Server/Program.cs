var builder = WebApplication.CreateBuilder(args);

var application = VertiqWebApplicationBuilder
    .CreateWithSerilog(
        "acme.vertiq.io.log",
        logger =>
            new ClocklyApplication(
                builder.Configuration,
                logger,
                builder.Services,
                builder.Environment.EnvironmentName
            )
    )
    .UseServerContext(builder)
    .UseModule<ClocklyServerModule>()
    .UseModuleWhenDevelopment<VertiqDiagnosticModule>()
    .BuildApplication();

var app = builder.Build();

application.ConfigurePipeline(app);

app.Run();

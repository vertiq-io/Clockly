using System.Threading.Tasks;
using System;

var sln = "Clockly.sln";

//Environment.SetEnvironmentVariable("GITHUB_ACTIONS", "1");

const string CHANGEME = "CHANGE ME";

var configuration = Environment.GetEnvironmentVariable("WEBDEPLOY_CONFIGURATION") ?? "Release";

var demoProjectName = "Clockly.Server";
var demoProjectDir = $"src/{demoProjectName}/";
var demoCSprojPath = Path.Combine(demoProjectDir, $"{demoProjectName}.csproj");

var iisPackageName = Environment.GetEnvironmentVariable("WEBDEPLOY_SITENAME") ?? "acme.vertiq.io";
var selfContained = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBDEPLOY_SELFHOST"))
    ? false
    : true;
var skipExtraFilesOnServer = !string.IsNullOrEmpty(
    Environment.GetEnvironmentVariable("WEBDEPLOY_REMOVEFILESONSERVER")
)
    ? false
    : true;

Target("restore", () => RunAsync("dotnet", $"restore {sln}"));
Target(
    "build",
    DependsOn("restore"),
    () => RunAsync("dotnet", $"build {sln} -c {configuration} --no-restore")
);
Target(
    "test",
    DependsOn("build"),
    () =>
        RunAsync(
            "dotnet",
            $"test {sln} --logger:\"console;verbosity=normal\" -c {configuration} --no-build -- xunit.parallelizeAssembly=true"
        )
);

Target(
    "run",
    DependsOn("build"),
    async () =>
    {
        await using var stdOut = Console.OpenStandardOutput();
        await using var stdErr = Console.OpenStandardError();

        var cmd =
            Cli.Wrap("dotnet").WithArguments($"""run --project "{demoCSprojPath}" """)
            | (stdOut, stdErr);

        await foreach (var cmdEvent in cmd.ListenAsync())
        {
            const string runningPattern = "Now listening on: ";

            string? uri = cmdEvent switch
            {
                StandardOutputCommandEvent e
                    => e.Text.StartsWith(runningPattern)
                        ? e.Text.Substring(runningPattern.Length).Trim()
                        : null,
                _ => null
            };
            if (uri is not null)
            {
                _ = LaunchBrowser(uri);
            }
        }

        static async Task LaunchBrowser(string url)
        {
            if (OperatingSystem.IsWindows())
            {
                await RunAsync("rundll32", $"url.dll,FileProtocolHandler {url}");
            }
            if (OperatingSystem.IsLinux())
            {
                await RunAsync("xdg-open", url);
            }
            if (OperatingSystem.IsMacOS())
            {
                await RunAsync("open", url);
            }
        }
    }
);

var GetWebdeployIP = () => Task.FromResult<string?>(
    Environment.GetEnvironmentVariable("WEBDEPLOY_IP") switch
    {
        string ip => ip,
        _ => "localhost"
    }
);

var GetWebdeployUser = () => Task.FromResult<string?>(
    Environment.GetEnvironmentVariable("WEBDEPLOY_USER") switch
    {
        string user => user,
        _ => "Administrator"
    }
);

var GetWebdeployPass = () => Task.FromResult<string?>(
    Environment.GetEnvironmentVariable("WEBDEPLOY_PASS") switch
    {
        string pass => pass,
        _ => CHANGEME
    }
);

BuildAndDeployIISProject(
    new IISDeployOptions(demoProjectName, iisPackageName)
    {
        DotnetCore = true,
        PathToCsproj = demoCSprojPath,
        Configuration = configuration,
        GetWebdeployIP = GetWebdeployIP,
        GetWebdeployUser = GetWebdeployUser,
        GetWebdeployPass = GetWebdeployPass
    },
    iisPackageName
);

Target("deploy:wizard", async () =>
{
    AnsiConsole.MarkupLine("Welcome to the interactive deployment wizard.");

    var packageName = AnsiConsole.Ask("Please enter the [green]Site-Name[/]?", iisPackageName);
    var ip = AnsiConsole.Ask("Please enter the [green]IP-Address[/]?", await GetWebdeployIP());
    var username = AnsiConsole.Ask("Please enter the [green]Username[/]?", await GetWebdeployUser());
    var password = AnsiConsole.Ask("Please enter the [green]Password[/]?", await GetWebdeployPass());
    var fileName = AnsiConsole.Ask("Please enter the [green]Filename[/]?", Environment.GetEnvironmentVariable("DEPLOY_FILENAME") switch
    {
        string f => f,
        _ => "deploy.bat"
    });
    var env = $"""
        SET WEBDEPLOY_SITENAME={packageName}
        SET WEBDEPLOY_IP={ip}
        SET WEBDEPLOY_USER={username}
        SET WEBDEPLOY_PASS={password}
        """;

    var file = $"""
        {env}
        SET DEPLOY_FILENAME=%~n0%~x0
        b.bat deploy:{packageName}
        """;

    var overwrite = false;

    var shouldWrite = AnsiConsole.Confirm($"""
        [white]This is the current configuration. [green]Is this correct?[/][/]
        [red]YOU SHOULD NOT CHECKIN THIS FILE INTO YOUR REPOSITORY[/]

        {file}
        """, true);


    if (File.Exists(fileName))
    {
        overwrite = AnsiConsole.Confirm($"The file {fileName} already exists. Do you want to overwrite the configuration?", true);
    }

    if (!shouldWrite)
    {
        return;
    }

    await File.WriteAllTextAsync(fileName, file);
    await File.WriteAllTextAsync($"configure.{fileName}", $"""
        {env}
        SET DEPLOY_FILENAME={fileName}
        b.bat deploy:wizard
        """);

    if (AnsiConsole.Confirm("[green]Do you want to deploy now?[/]", true))
    {
        await RunAsync(fileName);
    }
});

Target("default", DependsOn("test", $"publish:{iisPackageName}"));

await RunTargetsAndExitAsync(args);
@echo off
@pushd %~dp0
set DOTNET_CLI_UI_LANGUAGE=en
set DOTNET_CLI_TELEMETRY_OPTOUT=1

@dotnet run --project ".\build\build\build.csproj" --no-launch-profile -- %*
@popd
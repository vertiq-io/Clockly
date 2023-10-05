#!/usr/bin/env bash
set -euo pipefail
$HOME/.dotnet/dotnet run --project "./build/build/build.csproj" --no-launch-profile -- "$@"



 
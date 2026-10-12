param([switch]$Migrate)

$ErrorActionPreference = 'Stop'

$envFile = Join-Path $PSScriptRoot '.env'
if (-not (Test-Path -LiteralPath $envFile)) {
    throw 'backend/.env not found. Copy .env.example to .env and fill in your local settings.'
}

foreach ($line in Get-Content -LiteralPath $envFile) {
    $trimmed = $line.Trim()
    if (-not $trimmed -or $trimmed.StartsWith('#')) { continue }
    if ($trimmed -notmatch '^([A-Za-z_][A-Za-z0-9_]*)=(.*)$') {
        throw "Invalid .env line: $trimmed"
    }

    $name = $Matches[1]
    $value = $Matches[2].Trim()
    if ($value.Length -ge 2 -and (($value.StartsWith('"') -and $value.EndsWith('"')) -or ($value.StartsWith("'") -and $value.EndsWith("'")))) {
        $value = $value.Substring(1, $value.Length - 2)
    }
    [Environment]::SetEnvironmentVariable($name, $value, 'Process')
}

if ($Migrate) {
    dotnet ef database update --project (Join-Path $PSScriptRoot 'CampusEats.Api.csproj')
} else {
    dotnet run --project (Join-Path $PSScriptRoot 'CampusEats.Api.csproj')
}

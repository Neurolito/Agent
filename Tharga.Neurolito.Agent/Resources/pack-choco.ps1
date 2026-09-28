param(
    [Parameter(Mandatory = $true)]
    [string]$Version,

    # Optional overrides
    [string]$ProjectPath = "",
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",

    # IMPORTANT: Default to self-contained so target machines don't need a .NET runtime installed
    [switch]$SelfContained = $true
)

$ErrorActionPreference = "Stop"

Write-Host "== Building Chocolatey package version $Version ==" -ForegroundColor Cyan

# Resolve script root (where pack-choco.ps1 is located)
$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Definition

# Project root is 1 level up from Resources folder (your existing assumption)
$ProjectRoot = Split-Path -Parent $ScriptRoot

function Resolve-AgentProjectPath {
    param(
        [string]$RepoRoot,
        [string]$Hint
    )

    # 1) Explicit override wins
    if ($Hint) {
        $hintFull = Join-Path $RepoRoot $Hint
        if (Test-Path $hintFull) { return (Resolve-Path $hintFull).Path }
        if (Test-Path $Hint) { return (Resolve-Path $Hint).Path }
        throw "ProjectPath was provided but not found: $Hint"
    }

    # 2) Prefer the agent project in the expected location
    $defaultAgent = Join-Path $RepoRoot "Tharga.Neurolito.Agent\Tharga.Neurolito.Agent.csproj"
    if (Test-Path $defaultAgent) {
        return (Resolve-Path $defaultAgent).Path
    }

    # 3) Fallback: if exactly one csproj exists use it; else list and fail
    $csprojs = Get-ChildItem -Path $RepoRoot -Filter *.csproj -Recurse -File
    if ($csprojs.Count -eq 0) {
        throw "No .csproj found under repo root: $RepoRoot. Provide -ProjectPath."
    }
    if ($csprojs.Count -gt 1) {
        $list = ($csprojs.FullName | ForEach-Object { " - $_" }) -join "`n"
        throw "Multiple .csproj files found. Provide -ProjectPath. Found:`n$list"
    }
    return $csprojs[0].FullName
}

$AgentProject = Resolve-AgentProjectPath -RepoRoot $ProjectRoot -Hint $ProjectPath

# Paths
$PublishDir    = Join-Path $ProjectRoot "choco-temp\tools"
$ChocoTemp     = Join-Path $ProjectRoot "choco-temp"
$LicenseSrc    = Join-Path $ProjectRoot "LICENSE"
$InstallSrc    = Join-Path $ScriptRoot "choco\chocolateyInstall.ps1"
$UninstallSrc  = Join-Path $ScriptRoot "choco\chocolateyUninstall.ps1"
$NuspecSrc     = Join-Path $ScriptRoot "choco\neurolito-agent.nuspec"

Write-Host "Repo root:     $ProjectRoot" -ForegroundColor DarkGray
Write-Host "Agent csproj:  $AgentProject" -ForegroundColor DarkGray
Write-Host "PublishDir:    $PublishDir" -ForegroundColor DarkGray
Write-Host "SelfContained: $SelfContained" -ForegroundColor DarkGray

# Clean temp folder
if (Test-Path $ChocoTemp) {
    Remove-Item $ChocoTemp -Recurse -Force
}
New-Item -ItemType Directory -Path $PublishDir -Force | Out-Null

Write-Host "== Publishing .NET application ==" -ForegroundColor Green

$sc = if ($SelfContained) { "true" } else { "false" }

# NOTE: Self-contained ensures the target machine does NOT need .NET installed.
# We keep PublishSingleFile=false for simpler service execution and easier debugging.

dotnet publish `
    $AgentProject `
    -c $Configuration `
    -r $Runtime `
    --self-contained $sc `
    /p:PublishSingleFile=false `
    /p:PublishTrimmed=false `
    -o $PublishDir

# Sanity check: ensure an exe was produced
$exe = Get-ChildItem -Path $PublishDir -Filter *.exe -File -ErrorAction SilentlyContinue | Sort-Object Length -Descending | Select-Object -First 1
if (-not $exe) {
    throw "Publish succeeded but no .exe was found in: $PublishDir"
}
Write-Host "Published EXE: $($exe.Name) ($([Math]::Round($exe.Length/1MB, 1)) MB)" -ForegroundColor DarkGray

Write-Host "== Copying Chocolatey files ==" -ForegroundColor Green

Copy-Item $LicenseSrc (Join-Path $ChocoTemp "LICENSE.txt")
Copy-Item $InstallSrc (Join-Path $PublishDir "chocolateyInstall.ps1")
Copy-Item $UninstallSrc (Join-Path $PublishDir "chocolateyUninstall.ps1")
Copy-Item $NuspecSrc (Join-Path $ChocoTemp "neurolito-agent.nuspec")

Write-Host "== Packing with Chocolatey ==" -ForegroundColor Green

Push-Location $ChocoTemp
choco pack neurolito-agent.nuspec --version $Version
Pop-Location

Write-Host "== DONE! Package built: $ChocoTemp\neurolito-agent.$Version.nupkg ==" -ForegroundColor Cyan

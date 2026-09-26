$ErrorActionPreference = 'Stop'

$toolsDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$pp = Get-PackageParameters

# --- Constants / defaults ---
$company    = 'Thargelion AB'
$appFolder  = 'Tharga.Neurolito.Agent'
$installDir = Join-Path $env:ProgramFiles (Join-Path $company $appFolder)

# Agent service
$agentServiceName = 'NeurolitoAgentService'        # internal name (no spaces)
$agentDisplayName = 'Neurolito Agent Service'      # shown in Services.msc
$agentDescription = 'Neurolito Agent Service (Thargelion AB)'

function Assert-Admin {
    $isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()
    ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

    if (-not $isAdmin) {
        throw 'This package must be installed from an elevated PowerShell (Administrator).'
    }
}

function Get-ServiceSafe([string]$Name) {
    Get-Service -Name $Name -ErrorAction SilentlyContinue
}

function Copy-Payload {
    if (-not (Test-Path $installDir)) {
        New-Item -ItemType Directory -Force -Path $installDir | Out-Null
    }

    $exclude = @('chocolateyinstall.ps1', 'chocolateyuninstall.ps1', 'chocolateybeforemodify.ps1')

    Get-ChildItem -Path $toolsDir -Force | Where-Object {
        $exclude -notcontains $_.Name.ToLowerInvariant()
    } | ForEach-Object {
        Copy-Item -Path $_.FullName -Destination $installDir -Recurse -Force
    }
}

function Get-AppExePath {
    $candidates = Get-ChildItem -Path $installDir -Filter *.exe -File -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -notmatch '^chocolatey' }

    if (-not $candidates -or $candidates.Count -eq 0) {
        throw "No application .exe found in install directory: $installDir"
    }

    ($candidates | Sort-Object Length -Descending | Select-Object -First 1).FullName
}

function Invoke-Sc {
    param(
        [Parameter(Mandatory = $true)][string[]]$Args
    )

    $out = & sc.exe @Args 2>&1
    $code = $LASTEXITCODE

    if ($code -ne 0) {
        $joined = ($Args -join ' ')
        throw "sc.exe failed (exit $code) for: sc.exe $joined`nOutput:`n$out"
    }

    return $out
}

function Ensure-WindowsService {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$DisplayName,
        [Parameter(Mandatory = $true)][string]$Description,
        [Parameter(Mandatory = $true)][string]$BinaryPathName,
        [string]$DependOn
    )

    $svc = Get-ServiceSafe $Name

    if (-not $svc) {
        Write-Host "Creating service '$Name'..." -ForegroundColor Cyan
        New-Service -Name $Name -BinaryPathName $BinaryPathName -DisplayName $DisplayName -StartupType Automatic | Out-Null
        Invoke-Sc -Args @('description', $Name, $Description) | Out-Null
    } else {
        Write-Host "Updating service '$Name'..." -ForegroundColor Cyan
        # sc.exe config cannot reliably receive a binPath value that contains embedded quotes
        # via PowerShell argument splatting — it truncates at the first closing quote.
        # Write ImagePath and DisplayName directly to the registry instead (same effect).
        $regPath = "HKLM:\SYSTEM\CurrentControlSet\Services\$Name"
        Set-ItemProperty -Path $regPath -Name 'ImagePath'    -Value $BinaryPathName
        Set-ItemProperty -Path $regPath -Name 'DisplayName'  -Value $DisplayName
        Invoke-Sc -Args @('config', $Name, 'start=', 'auto') | Out-Null
        Invoke-Sc -Args @('description', $Name, $Description) | Out-Null
    }

    if ($DependOn) {
        $depSvc = Get-ServiceSafe $DependOn
        if ($depSvc) {
            Invoke-Sc -Args @('config', $Name, 'depend=', $DependOn) | Out-Null
        } else {
            Write-Warning "DependOnService '$DependOn' was specified, but no such Windows service exists. Skipping dependency."
        }
    }

    # Restart on failure
    Invoke-Sc -Args @('failure', $Name, 'reset=', '86400', 'actions=', 'restart/5000/restart/5000/restart/5000') | Out-Null

    $svc = Get-ServiceSafe $Name
    if (-not $svc) {
        $q = & sc.exe query $Name 2>&1
        throw "Service '$Name' still not visible after creation/update.`nsc.exe query output:`n$q"
    }
}

# --------------------
# Install flow
# --------------------
Assert-Admin

# Stop existing agent service if present (upgrade/reinstall scenario)
$existing = Get-ServiceSafe $agentServiceName
if ($existing -and $existing.Status -eq 'Running') {
    Write-Host "Stopping '$agentServiceName' before update..." -ForegroundColor Yellow
    Stop-Service -Name $agentServiceName -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
}

Write-Host "Installing payload to: $installDir" -ForegroundColor Green
Copy-Payload

$exePath = Get-AppExePath

# Params:
#   --params "'/Urls=http://127.0.0.1:5001'"
#   --params "'/Port=5001'"
#   --params "'/DependOnService=SomeServiceName'"
#   --params "'/ApiKey=<team install key>'"
$urls = if ($pp.Urls) { $pp.Urls } elseif ($pp.Port) { "http://127.0.0.1:$($pp.Port)" } else { 'http://127.0.0.1:5001' }
$dependOnService = if ($pp.DependOnService) { $pp.DependOnService } else { $null }

function Write-AgentApiKey {
    param([string]$ApiKey)

    # The key the agent presents when it connects, identifying the team that owns this machine.
    # Written to its own file rather than into appsettings.json, for two reasons: the package
    # overwrites appsettings.json on every upgrade, which would silently unassign the agent; and a
    # secret in the file everyone opens to change a log level is a secret that ends up in a paste.
    $configPath = Join-Path $installDir 'appsettings.Agent.json'

    if (-not $ApiKey) {
        # Nothing passed. An existing key is kept - an upgrade that ran without the parameter must
        # not disown a machine that was correctly configured before it.
        if (Test-Path $configPath) {
            Write-Host 'No /ApiKey given; keeping the key already on this machine.' -ForegroundColor DarkGray
        } else {
            Write-Warning 'No /ApiKey given. This agent will connect without naming a team and will not be able to run any jobs. Re-run the install command from the Install an agent page.'
        }
        return
    }

    $config = @{ Tharga = @{ Communication = @{ ApiKey = $ApiKey } } }
    $json = $config | ConvertTo-Json -Depth 5

    Set-Content -Path $configPath -Value $json -Encoding UTF8

    # Readable by the service account and administrators, and by nobody else. The file holds a
    # credential that can join a machine to the team's fleet.
    $acl = Get-Acl $configPath
    $acl.SetAccessRuleProtection($true, $false)
    $acl.Access | ForEach-Object { $acl.RemoveAccessRule($_) | Out-Null }
    foreach ($identity in @('NT AUTHORITY\SYSTEM', 'BUILTIN\Administrators')) {
        $rule = New-Object System.Security.AccessControl.FileSystemAccessRule($identity, 'FullControl', 'Allow')
        $acl.AddAccessRule($rule)
    }
    Set-Acl -Path $configPath -AclObject $acl

    Write-Host "Agent key written to $configPath" -ForegroundColor DarkGray
}

# IMPORTANT: service content root
$binaryPath = '"' + $exePath + '"' + ' --urls "' + $urls + '" --contentRoot "' + $installDir + '"'

# Avoid PowerShell line continuations here (they can be fragile inside Chocolatey runners)
$svcArgs = @{
    Name          = $agentServiceName
    DisplayName   = $agentDisplayName
    Description   = $agentDescription
    BinaryPathName= $binaryPath
    DependOn      = $dependOnService
}

Write-AgentApiKey -ApiKey $pp.ApiKey

Ensure-WindowsService @svcArgs

Write-Host "Starting service '$agentServiceName'..." -ForegroundColor Green
Start-Service -Name $agentServiceName -ErrorAction Stop

Start-Sleep -Seconds 2
(Get-ServiceSafe $agentServiceName) | Format-Table -AutoSize

Write-Host "Installed executable: $exePath" -ForegroundColor DarkGray
Write-Host "Binding URLs: $urls" -ForegroundColor DarkGray

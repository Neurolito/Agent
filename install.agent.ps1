<#
.SYNOPSIS
    Installs and runs the Tharga Neurolito Agent as a Docker container.

.DESCRIPTION
    The image is pulled through the Neurolito site with your team's install key, and the agent is
    attached to that team. This is the route the install page describes.

    With -NoPull the image is built from this repository instead.

.PARAMETER Site
    Host name of the Neurolito site to pull through. Default: neurolito.com.

.PARAMETER ApiKey
    Your team's install key, from the install page. It attaches the agent to your team and is the
    credential for the pull. Prompted for when omitted. Passed to the container through the
    environment, never on the docker command line.

.PARAMETER Environment
    ASPNETCORE_ENVIRONMENT value passed into the container.

.PARAMETER Name
    Container name (and hostname). If omitted, you'll be prompted. Default is <MachineName>1.

.PARAMETER Port
    Host port mapped to container port 5000. If omitted, you'll be prompted. Default is 5101.

.PARAMETER Gpu
    If specified, runs container with --gpus=all.

.PARAMETER OllamaPort
    Optional host port to expose Ollama from inside the container.

    If provided, maps host port <OllamaPort> to container port 11434.
    If omitted, Ollama is NOT exposed externally.

.PARAMETER NoPull
    If specified, builds the image from this repository instead of pulling it.

.PARAMETER Tag
    Image tag to pull. Default: latest.

.EXAMPLE
    # Pull through the site and attach the agent to your team (prompts for the key)
    .\install.agent.ps1

.EXAMPLE
    # Run on the GPU
    .\install.agent.ps1 -Gpu

.EXAMPLE
    # Build from source instead of pulling
    .\install.agent.ps1 -NoPull

.NOTES
    For full help:
      Get-Help .\install.agent.ps1 -Detailed
      Get-Help .\install.agent.ps1 -Examples
      .\install.agent.ps1 -?
#>

[CmdletBinding()]
param(
    [string]$Site = "neurolito.com",

    [string]$ApiKey,

    [string]$Environment = "Production",

    # If not provided (or empty), script will prompt with a default.
    [string]$Name,

    # If not provided, script will prompt with a default (5101).
    [int]$Port = 5101,

    [switch]$Gpu,

    # Optional: expose Ollama (container port 11434) on a chosen host port.
    # If omitted, Ollama is not exposed.
    [int]$OllamaPort,

    # Default is to PULL. Use -NoPull to build from this repository instead.
    [switch]$NoPull,

    [string]$Tag = "latest"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# PS7: prevent native commands (docker) from turning non-zero exit codes into terminating PS errors
Set-Variable -Name PSNativeCommandUseErrorActionPreference -Scope Global -Value $false -Force -ErrorAction SilentlyContinue | Out-Null

function Write-Info([string]$Message) { Write-Host "[INFO] $Message" -ForegroundColor Cyan }
function Write-Warn([string]$Message) { Write-Host "[WARN] $Message" -ForegroundColor Yellow }
function Write-Err([string]$Message)  { Write-Host "[ERR ] $Message" -ForegroundColor Red }

function Invoke-External {
    param(
        [Parameter(Mandatory = $true)][string]$File,
        [Parameter(Mandatory = $true)][string[]]$Arguments
    )

    Write-Info ("{0} {1}" -f $File, ($Arguments -join " "))
    & $File @Arguments

    if ($LASTEXITCODE -ne 0) {
        throw ("{0} failed with exit code {1}" -f $File, $LASTEXITCODE)
    }
}

function Remove-ContainerIfExists([string]$ContainerName) {
    Write-Info "Removing existing container (if any)..."
    try {
        & docker rm -f $ContainerName 1>$null 2>$null
    }
    catch {
        if ($_.Exception.Message -notmatch "No such container") { throw }
    }
    finally {
        $global:LASTEXITCODE = 0
    }
}

function Wait-ContainerRunning {
    param(
        [Parameter(Mandatory=$true)][string]$ContainerName,
        [int]$TimeoutSeconds = 20
    )

    $start = Get-Date
    while ($true) {
        try {
            $state = (& docker inspect -f "{{.State.Status}}" $ContainerName 2>$null).Trim()
            $global:LASTEXITCODE = 0

            if ($state -eq "running") {
                Write-Info "Container is running."
                return
            }

            if ($state -eq "exited" -or $state -eq "dead") {
                throw "Container started but is not running (state: $state). Run 'docker logs $ContainerName' to see why."
            }
        }
        catch {
            $global:LASTEXITCODE = 0
        }

        if (((Get-Date) - $start).TotalSeconds -ge $TimeoutSeconds) {
            Write-Warn "Timed out waiting for container to enter 'running' state. Check: docker ps -a ; docker logs $ContainerName"
            return
        }

        Start-Sleep -Milliseconds 500
    }
}

function Prompt-ForNameAndPortIfMissing {
    param(
        [ref]$NameRef,
        [ref]$PortRef
    )

    $machine = [System.Environment]::MachineName
    $defaultName = "${machine}1"
    $defaultPort = 5101

    # NAME: prompt if missing OR empty/whitespace
    if ([string]::IsNullOrWhiteSpace($NameRef.Value)) {
        while ($true) {
            $entered = Read-Host "Name [$defaultName]"
            if ([string]::IsNullOrWhiteSpace($entered)) {
                $NameRef.Value = $defaultName
            }
            else {
                $NameRef.Value = $entered.Trim()
            }

            if (-not [string]::IsNullOrWhiteSpace($NameRef.Value)) { break }
            Write-Warn "Name cannot be empty."
        }
    }

    # PORT: prompt only if -Port wasn't provided explicitly
    if (-not $PSBoundParameters.ContainsKey('Port')) {
        while ($true) {
            $entered = Read-Host "Port [$defaultPort]"
            if ([string]::IsNullOrWhiteSpace($entered)) {
                $PortRef.Value = $defaultPort
                break
            }

            $p = 0
            if (-not [int]::TryParse($entered.Trim(), [ref]$p)) {
                Write-Warn "Port must be a number."
                continue
            }
            if ($p -lt 1 -or $p -gt 65535) {
                Write-Warn "Port must be between 1 and 65535."
                continue
            }

            $PortRef.Value = $p
            break
        }
    }
}

try {
    if (-not (Get-Command docker -ErrorAction SilentlyContinue)) { throw "Docker is not installed or not in PATH." }

    # Prompt for missing inputs
    Prompt-ForNameAndPortIfMissing -NameRef ([ref]$Name) -PortRef ([ref]$Port)

    # Validate port (covers the case where -Port was provided explicitly)
    if ($Port -lt 1 -or $Port -gt 65535) { throw "Port must be between 1 and 65535." }

    # Validate optional Ollama port (only when explicitly provided)
    if ($PSBoundParameters.ContainsKey('OllamaPort')) {
        if ($OllamaPort -lt 1 -or $OllamaPort -gt 65535) {
            throw "OllamaPort must be between 1 and 65535. Omit -OllamaPort to keep it unexposed."
        }
    }

    # Container + hostname handling
    $baseName = "tharga-neurolito-agent"
    $containerName = $baseName
    $hostnameArgs = @()
    $hostnameLabel = $baseName

    if (-not [string]::IsNullOrWhiteSpace($Name)) {
        $containerName = $Name
        $hostnameArgs = @("--hostname", $Name)
        $hostnameLabel = $Name
    }

    $gpuArgs = @()
    if ($Gpu.IsPresent) {
        $gpuArgs = @("--gpus=all")
    }

    if ($NoPull.IsPresent) {
        $imageName = "tharga-neurolito-agent:local"
        $modeText = "Local build"
        Write-Info "Building local image..."
        Invoke-External -File "docker" -Arguments @("build", "-f", (Join-Path $PSScriptRoot "Tharga.Neurolito.Agent/Dockerfile"), "-t", $imageName, $PSScriptRoot)
    }
    else {
        if ([string]::IsNullOrWhiteSpace($Site)) { throw "-Site cannot be empty." }
        if ([string]::IsNullOrWhiteSpace($ApiKey)) {
            $secure = Read-Host "Team install key (from the Install an agent page)" -AsSecureString
            $ApiKey = [System.Net.NetworkCredential]::new("", $secure).Password
        }
        if ([string]::IsNullOrWhiteSpace($ApiKey)) { throw "Pulling through the site needs the team install key." }

        $siteHost = $Site.Trim() -replace '^https?://', '' -replace '/+$', ''

        # The username is ignored by the site; only the key identifies the team.
        Write-Info ("docker login {0} --username neurolito --password-stdin" -f $siteHost)
        $ApiKey | & docker login $siteHost --username neurolito --password-stdin
        if ($LASTEXITCODE -ne 0) { throw "docker login to $siteHost failed. Check the install key on the Install an agent page." }

        $imageName = ("{0}/neurolito-agent:{1}" -f $siteHost, $Tag)
        $modeText  = ("Pull through {0}" -f $siteHost)

        Write-Info "Pulling image..."
        Invoke-External -File "docker" -Arguments @("pull", $imageName)
    }

    Write-Info "Settings:"
    Write-Info ("  ASPNETCORE_ENVIRONMENT = {0}" -f $Environment)
    Write-Info ("  Container name         = {0}" -f $containerName)
    Write-Info ("  Hostname               = {0}" -f $hostnameLabel)
    Write-Info ("  Port mapping           = {0} -> 5000" -f $Port)
    Write-Info ("  GPU                    = {0}" -f $Gpu.IsPresent)
    Write-Info ("  Mode                   = {0}" -f $modeText)
    Write-Info ("  Image                  = {0}" -f $imageName)

    if ($PSBoundParameters.ContainsKey('OllamaPort')) {
        Write-Info ("  Ollama port            = {0} -> 11434" -f $OllamaPort)
    }
    else {
        Write-Info "  Ollama port            = NOT EXPOSED externally"
    }

    Remove-ContainerIfExists -ContainerName $containerName

    # Start container detached
    $portArgs = @(
        # Localhost only: the agent's local API has no authentication.
        "-p", ("127.0.0.1:{0}:5000" -f $Port)
    )

    if ($PSBoundParameters.ContainsKey('OllamaPort')) {
        $portArgs += @(
            "-p", ("127.0.0.1:{0}:11434" -f $OllamaPort)
        )
    }

    # Models live in a named volume, so recreating the container for an upgrade keeps them.
    # The restart policy brings the agent back after a reboot, as the Windows service does.
    $runArgs = @("run", "-d", "--name", $containerName, "--restart", "unless-stopped", "-v", "neurolito-ollama:/root/.ollama") + $hostnameArgs + $gpuArgs + $portArgs + @(
        "-e", "ASPNETCORE_URLS=http://0.0.0.0:5000",
        "-e", ("ASPNETCORE_ENVIRONMENT={0}" -f $Environment)
    )

    # Name only: docker copies the value from this process's environment, so the key never
    # appears on the command line or in the echoed arguments.
    if (-not [string]::IsNullOrWhiteSpace($ApiKey)) {
        $env:Tharga__Communication__ApiKey = $ApiKey
        $runArgs += @("-e", "Tharga__Communication__ApiKey")
    }
    else {
        Write-Warn "No -ApiKey given. The agent will connect without naming a team and cannot run jobs."
    }

    $runArgs += @($imageName)

    try {
        Invoke-External -File "docker" -Arguments $runArgs
    }
    finally {
        Remove-Item Env:\Tharga__Communication__ApiKey -ErrorAction SilentlyContinue
    }

    Wait-ContainerRunning -ContainerName $containerName -TimeoutSeconds 20

    Write-Info ("URL: http://localhost:{0}" -f $Port)
    Write-Info "Done. Container is started in the background; you can keep using this terminal."
    Write-Info ("Logs: docker logs -f {0}" -f $containerName)
}
catch {
    Write-Err $_.Exception.Message
    exit 1
}

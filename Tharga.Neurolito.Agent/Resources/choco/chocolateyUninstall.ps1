$ErrorActionPreference = 'Stop'

$company    = 'Thargelion AB'
$appFolder  = 'Tharga.Neurolito.Agent'
$installDir = Join-Path $env:ProgramFiles (Join-Path $company $appFolder)

$agentServiceName = 'NeurolitoAgentService'

function Get-ServiceSafe([string]$Name) {
    Get-Service -Name $Name -ErrorAction SilentlyContinue
}

$svc = Get-ServiceSafe $agentServiceName
if ($svc) {
    try {
        if ($svc.Status -eq 'Running') {
            Write-Host "Stopping service '$agentServiceName'..."
            Stop-Service -Name $agentServiceName -Force -ErrorAction SilentlyContinue
            Start-Sleep -Seconds 2
        }
    } catch {
        Write-Warning "Failed to stop service '$agentServiceName': $($_.Exception.Message)"
    }

    try {
        Write-Host "Deleting service '$agentServiceName'..."
        & sc.exe delete $agentServiceName | Out-Null
    } catch {
        Write-Warning "Failed to delete service '$agentServiceName': $($_.Exception.Message)"
    }
} else {
    Write-Host "Service '$agentServiceName' not found. Skipping service removal."
}

if (Test-Path $installDir) {
    try {
        Write-Host "Removing install directory: $installDir"
        Remove-Item -Path $installDir -Recurse -Force
    } catch {
        Write-Warning "Failed to remove install directory '$installDir': $($_.Exception.Message)"
    }
}

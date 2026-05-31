param(
    [string]$DropletIp,
    [string]$User = "deploy",
    [string]$RepoUrl = "https://github.com/krishpdsharma/minepress.erp.git"
)

if (-not $DropletIp) {
    Write-Error "Droplet IP is required: .\deploy\deploy.ps1 -DropletIp <ip>"
    exit 1
}

$remoteDir = "/srv/minepress"

Write-Host "Copying files to $DropletIp..."
# Archive current repo and copy to remote
$archive = "$(Split-Path -Parent $MyInvocation.MyCommand.Definition)\..\deploy\deploy.tar.gz"
if (Test-Path $archive) { Remove-Item $archive }

Write-Host "Creating archive..."
& tar -czf $archive -C .. .

Write-Host "Uploading archive to droplet..."
scp $archive $User@$DropletIp:/tmp/deploy.tar.gz

$ssh = "ssh $User@$DropletIp"
Write-Host "Extracting on remote..."
Invoke-Expression "$ssh 'sudo mkdir -p $remoteDir && sudo tar -xzf /tmp/deploy.tar.gz -C $remoteDir && sudo chown -R $User:$User $remoteDir'"

Write-Host "Starting docker-compose on remote..."
Invoke-Expression "$ssh 'cd $remoteDir && docker compose up -d --build'"

Write-Host "Deployment finished. Open http://$DropletIp in your browser (or configure DNS)."
    [int]$SshPort = 22,
    [string]$KeyFile = "",
    [string]$EnvFile = "",
    [switch]$SkipBuild
)

Write-Host "Creating archive of repository..."
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$repoRoot = Resolve-Path (Join-Path $scriptDir "..")
$archive = Join-Path $scriptDir "deploy.tar.gz"
if (Test-Path $archive) { Remove-Item $archive -Force }

# create a tar.gz archive of the repository root
Push-Location $repoRoot
& tar -czf $archive .
Pop-Location

Write-Host "Uploading archive to $User@$DropletIp..."

# Build scp arguments
$scpArgs = @()
if ($KeyFile) { $scpArgs += "-i"; $scpArgs += $KeyFile }
if ($SshPort -ne 22) { $scpArgs += "-P"; $scpArgs += $SshPort.ToString() }
$scpArgs += $archive
$scpArgs += "$User@$DropletIp:/tmp/deploy.tar.gz"

try {
    & scp @scpArgs
} catch {
    Write-Error "SCP failed: $_"
    exit 1
}

# Build base ssh args (we pass the remote command as the last arg)
$sshBase = @()
if ($KeyFile) { $sshBase += "-i"; $sshBase += $KeyFile }
if ($SshPort -ne 22) { $sshBase += "-p"; $sshBase += $SshPort.ToString() }
$sshBase += "$User@$DropletIp"

Write-Host "Extracting archive on remote and preparing directory..."
$remoteCmd = "sudo mkdir -p $remoteDir && sudo tar -xzf /tmp/deploy.tar.gz -C $remoteDir && sudo chown -R $User:$User $remoteDir"
try {
    & ssh @($sshBase + @($remoteCmd))
} catch {
    Write-Error "SSH extract failed: $_"
    exit 1
}

if ($EnvFile) {
    if (-not (Test-Path $EnvFile)) {
        Write-Warning "Env file '$EnvFile' not found locally; skipping upload."
    } else {
        Write-Host "Uploading env file to remote..."
        $scpEnvArgs = @()
        if ($KeyFile) { $scpEnvArgs += "-i"; $scpEnvArgs += $KeyFile }
        if ($SshPort -ne 22) { $scpEnvArgs += "-P"; $scpEnvArgs += $SshPort.ToString() }
        $scpEnvArgs += $EnvFile
        $scpEnvArgs += "$User@$DropletIp:$remoteDir/.env"
        & scp @scpEnvArgs
    }
}

Write-Host "Starting docker-compose on remote..."
$composeCmd = "cd $remoteDir && docker compose up -d"
if (-not $SkipBuild) { $composeCmd = "$composeCmd --build" }
try {
    & ssh @($sshBase + @($composeCmd))
} catch {
    Write-Error "SSH docker-compose failed: $_"
    exit 1
}


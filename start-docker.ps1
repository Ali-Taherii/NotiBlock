# NotiBlock Docker Quick Start Script
# This script sets up and starts the entire NotiBlock stack with one command

param(
    [ValidateSet('up', 'down', 'logs', 'restart', 'status', 'clean')]
    [string]$Command = 'up',
    
    [switch]$Build,
    [switch]$Detach
)

$projectRoot = Split-Path -Parent $MyInvocation.MyCommandPath

function Write-Header {
    param([string]$Text)
    Write-Host "`n=== $Text ===" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Text)
    Write-Host "✓ $Text" -ForegroundColor Green
}

function Write-Error {
    param([string]$Text)
    Write-Host "✗ $Text" -ForegroundColor Red
}

function Write-Info {
    param([string]$Text)
    Write-Host "ℹ $Text" -ForegroundColor Yellow
}

# Check if Docker is installed and running
function Test-Docker {
    try {
        $null = docker ps 2>&1
        return $true
    }
    catch {
        return $false
    }
}

# Check if .env file exists
function Test-EnvFile {
    return Test-Path (Join-Path $projectRoot '.env')
}

# Create .env from template
function Initialize-Env {
    $envExample = Join-Path $projectRoot '.env.example'
    $envFile = Join-Path $projectRoot '.env'
    
    if (-not (Test-Path $envFile)) {
        Write-Header "Initializing Environment"
        
        if (Test-Path $envExample) {
            Copy-Item $envExample $envFile
            Write-Success ".env file created from template"
            Write-Info "Edit .env file with your configuration values"
            Write-Info "Required: DB_PASSWORD, JWT_KEY"
            
            # Open .env in default editor if on Windows
            if ([System.Environment]::OSVersion.Platform -eq 'Win32NT') {
                notepad $envFile
            }
        }
        else {
            Write-Error ".env.example not found!"
            exit 1
        }
    }
    else {
        Write-Success ".env file exists"
    }
}

function Start-Stack {
    Write-Header "Starting NotiBlock Stack"
    
    $args = @('up', '-d')
    if ($Build) {
        $args += '--build'
        Write-Info "Rebuilding images..."
    }
    
    docker-compose -f "$projectRoot/docker-compose.yml" $args
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Stack started successfully"
        Show-Status
    }
    else {
        Write-Error "Failed to start stack"
        exit 1
    }
}

function Stop-Stack {
    Write-Header "Stopping NotiBlock Stack"
    docker-compose -f "$projectRoot/docker-compose.yml" down
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Stack stopped"
    }
    else {
        Write-Error "Failed to stop stack"
    }
}

function Show-Status {
    Write-Header "Service Status"
    docker-compose -f "$projectRoot/docker-compose.yml" ps
    
    Write-Header "Access Points"
    Write-Host "Frontend:      http://localhost (or http://localhost:3000)" -ForegroundColor Green
    Write-Host "Backend API:   http://localhost:5271" -ForegroundColor Green
    Write-Host "Health Check:  http://localhost:5271/api/auth/health" -ForegroundColor Green
    Write-Host "Database:      localhost:5432" -ForegroundColor Green
}

function Show-Logs {
    Write-Header "Streaming Logs (Ctrl+C to exit)"
    docker-compose -f "$projectRoot/docker-compose.yml" logs -f --tail=50
}

function Restart-Stack {
    Write-Header "Restarting NotiBlock Stack"
    docker-compose -f "$projectRoot/docker-compose.yml" restart
    
    Write-Success "Stack restarted"
    Start-Sleep -Seconds 2
    Show-Status
}

function Clean-Stack {
    Write-Header "Cleaning NotiBlock Stack"
    Write-Info "This will remove all containers and data. Type 'yes' to confirm:"
    
    $confirm = Read-Host "Confirm (yes/no)"
    if ($confirm -eq 'yes') {
        docker-compose -f "$projectRoot/docker-compose.yml" down -v
        Write-Success "Stack cleaned"
    }
    else {
        Write-Info "Cancelled"
    }
}

# Main logic
if (-not (Test-Docker)) {
    Write-Error "Docker is not installed or not running!"
    Write-Info "Install Docker Desktop: https://docs.docker.com/desktop/install/"
    exit 1
}

Write-Header "NotiBlock Docker Manager"

Initialize-Env

switch ($Command) {
    'up' { Start-Stack }
    'down' { Stop-Stack }
    'logs' { Show-Logs }
    'status' { Show-Status }
    'restart' { Restart-Stack }
    'clean' { Clean-Stack }
    default {
        Write-Error "Unknown command: $Command"
        Write-Info "Valid commands: up, down, logs, status, restart, clean"
    }
}

Write-Host ""

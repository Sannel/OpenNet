#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Installs OpenNet.Agent as a Windows Service.
.DESCRIPTION
    Publishes OpenNet.Agent as a self-contained executable and installs it as a Windows Service.
#>

param(
    [string]$InstallPath = "C:\Program Files\OpenNet",
    [string]$ServiceName = "OpenNet",
    [string]$ServiceDisplayName = "OpenNet Agent Service",
    [string]$ServiceDescription = "OpenNet Agent background service"
)

$ErrorActionPreference = "Stop"

Write-Host "Publishing OpenNet.Agent..."
dotnet publish src/OpenNet.Agent/OpenNet.Agent.csproj -c Release -r win-x64 --self-contained -o $InstallPath

Write-Host "Creating Windows Service..."
$existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($null -ne $existingService)
{
    Write-Host "Stopping existing service..."
    Stop-Service -Name $ServiceName -Force
    sc.exe delete $ServiceName
    Start-Sleep -Seconds 2
}

$exePath = Join-Path $InstallPath "OpenNet.Agent.exe"
New-Service -Name $ServiceName `
    -DisplayName $ServiceDisplayName `
    -Description $ServiceDescription `
    -BinaryPathName $exePath `
    -StartupType Automatic

Write-Host "Starting service..."
Start-Service -Name $ServiceName

Write-Host "OpenNet Agent service installed and started successfully."
Write-Host "Service name: $ServiceName"
Write-Host "Install path: $InstallPath"

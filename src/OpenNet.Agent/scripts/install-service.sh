#!/usr/bin/env bash
set -euo pipefail

INSTALL_PATH="/opt/opennet"
SERVICE_NAME="opennet"
SERVICE_USER="opennet"

if [ "$EUID" -ne 0 ]; then
    echo "Please run as root (sudo)"
    exit 1
fi

echo "Publishing OpenNet.Agent..."
dotnet publish src/OpenNet.Agent/OpenNet.Agent.csproj -c Release -r linux-x64 --self-contained -o "$INSTALL_PATH"

echo "Creating service user if not exists..."
if ! id -u "$SERVICE_USER" &>/dev/null; then
    useradd --system --no-create-home --shell /sbin/nologin "$SERVICE_USER"
fi

echo "Setting permissions..."
chown -R "$SERVICE_USER:$SERVICE_USER" "$INSTALL_PATH"
chmod +x "$INSTALL_PATH/OpenNet.Agent"

echo "Installing systemd service..."
cp opennet.service /etc/systemd/system/opennet.service

echo "Reloading systemd..."
systemctl daemon-reload
systemctl enable "$SERVICE_NAME"
systemctl restart "$SERVICE_NAME"

echo "OpenNet Agent service installed and started successfully."
systemctl status "$SERVICE_NAME"

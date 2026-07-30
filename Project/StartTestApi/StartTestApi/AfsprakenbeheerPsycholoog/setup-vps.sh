#!/bin/bash
# Auto-setup script voor De Verstandhouding op Combell VPS (136.144.213.141)

set -e

echo "========================================================"
echo " Starting De Verstandhouding VPS Setup (136.144.213.141)"
echo "========================================================"

# Update system
sudo apt-get update && sudo apt-get upgrade -y

# Install Docker & Docker Compose if not installed
if ! command -v docker &> /dev/null; then
    echo "Installing Docker..."
    curl -fsSL https://get.docker.com -o get-docker.sh
    sudo sh get-docker.sh
    sudo usermod -aG docker $USER
    rm get-docker.sh
fi

if ! command -v docker-compose &> /dev/null; then
    echo "Installing Docker Compose..."
    sudo apt-get install -y docker-compose-plugin
fi

# Configure firewall (UFW)
echo "Configuring firewall..."
sudo ufw allow 22/tcp
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw --force enable

# Build & Start Container Stack
echo "Building and starting De Verstandhouding containers..."
sudo docker compose up -d --build

echo "========================================================"
echo " Setup Complete! De Verstandhouding is live op:"
echo " https://www.deverstandhouding.be"
echo "========================================================"

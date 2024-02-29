#!/bin/sh


sudo apt-get update
sudo apt install jq -y
sudo apt install network-manager -y
sudo apt install redsocks -y
sudo apt install microsocks -y
sudo apt install proxychains -y
sudo apt install mptcpize -y
sudo apt install iptables -y

sudo cp mptcp-wsl.service /etc/systemd/system/
sudo systemctl start mptcp-wsl.service
sudo systemctl enable mptcp-wsl.service

sudo systemctl disable redsocks
sudo systemctl disable mptcp.service
sudo cp mptcp-redsocks.conf /etc/
sudo mkdir /usr/local/bin/mptcp
sudo cp mptcp-wsl.sh /usr/local/bin/mptcp/
sudo chmod +x /usr/local/bin/mptcp/mptcp-wsl.sh

sudo chmod +x iface_setup
sudo cp iface_setup /etc/NetworkManager/dispatcher.d/
sudo cp mptcp-NetworkManager.conf /etc/NetworkManager/conf.d/
sudo systemctl restart NetworkManager

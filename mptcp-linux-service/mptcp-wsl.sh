#!/bin/sh

trap 'cleanup' INT TERM


cleanup(){
    kill "$redsocks_PID"
    kill "$microsocks1_PID"
    kill "$microsocks2_PID"
}

search_powershell() {
    local powershell_path
    for drive in /mnt/*; do
        if [ -d "$drive" ]; then
            powershell_path="$drive/Windows/System32/WindowsPowerShell/v1.0/powershell.exe"
            if [ -e "$powershell_path" ]; then
                echo "$powershell_path"
            fi
        fi
    done
}

config_limits(){
      local powershell_path=$(search_powershell)
      user_path=$(sudo -u $(whoami) wslpath "$($powershell_path '$env:USERPROFILE')" | tr -cd '[:alnum:]/')/AppData/Local/MPTCP/config.json
      SubflowNr=$(jq ".SubflowNr" "$user_path")
      AddAddrAcceptedNr=$(jq ".AddAddrAcceptedNr" "$user_path")
      sudo ip mptcp limits set subflow "$SubflowNr"
      sudo ip mptcp limits set add_addr_accepted "$AddAddrAcceptedNr"
}
config_proxy() {
    local powershell_path=$(search_powershell)
    user_path=$(sudo -u $(whoami) wslpath "$($powershell_path '$env:USERPROFILE')" | tr -cd '[:alnum:]/')/AppData/Local/MPTCP/config.json    
    local proxy_address=$(jq -r '.Proxy.proxyAddress' "$user_path")
    local proxy_port=$(jq -r '.Proxy.proxyPort' "$user_path")
    local user=$(jq -r '.Proxy.user' "$user_path")
    local password=$(jq -r '.Proxy.password' "$user_path")
    cat > /etc/proxychains.conf <<EOF
strict_chain
proxy_dns
tcp_read_time_out 15000
tcp_connect_time_out 8000

[ProxyList]
socks5 127.0.0.1 1081
socks5 $proxy_address $proxy_port $user $password
EOF
}

stop_proxy(){
    sudo pkill -f "redsocks -c /etc/mptcp-redsocks.conf"
    sudo pkill -f "proxychains microsocks -p 1080"
    sudo pkill -f "mptcpize run microsocks -p 1081"
}

delete_iptable(){
    sudo iptables -t nat -D PREROUTING -p tcp --dport 443 -j REDSOCKS
    sudo iptables -t nat -D PREROUTING -p tcp --dport 80 -j REDSOCKS
    sudo iptables -t nat -F REDSOCKS
    sudo iptables -t nat -X REDSOCKS
}

config_iptable(){
    sudo iptables -t nat -N REDSOCKS
    sudo iptables -t nat -A REDSOCKS -p tcp -j REDIRECT --to-ports 12345
    sudo iptables -t nat -A PREROUTING -p tcp --dport 443 -j REDSOCKS
    sudo iptables -t nat -A PREROUTING -p tcp --dport 80 -j REDSOCKS
}

stop_proxy
delete_iptable

config_limits
config_iptable
config_proxy
redsocks -c /etc/mptcp-redsocks.conf &
redsocks_PID=$!
proxychains microsocks -p 1080 &
microsocks1_PID=$!
mptcpize run microsocks -p 1081 &
microsocks2_PID=$!
wait

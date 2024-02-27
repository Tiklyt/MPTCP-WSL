import pyudev
import subprocess
import netifaces
import json

table = 1
priority = 100


def disable_eth0():
    subprocess.run(["ip", "link", "set", "eth0", "down"])


def disable_mptcp_service():
    subprocess.run(["systemctl", "stop", "mptcp.service"])


def read_config():
    path = get_config_path()
    with open(path, 'r') as file:
        return json.load(file)


def apply_general_config(config):
    disable_eth0()
    disable_mptcp_service()
    change_dns(config["DnsServer"])
    # change_subflow_limit(config)
    # change_addr_accepted(config)
    prioritize_ipv4()


def get_entry_from_config(config, macAddress):
    for entry in config["Config"]:
        revertMac = revert(entry["MacAddress"]).lower()
        if revertMac == macAddress:
            return entry


def transform(original_mac):
    key = [0x42, 0x37, 0x29, 0x15, 0x5A, 0x3C]

    original_bytes = parse_mac_address(original_mac)
    transformed_bytes = [original_byte ^ key_byte for original_byte, key_byte in zip(original_bytes, key)]

    return format_mac_address(transformed_bytes)


def revert(transformed_mac):
    return transform(transformed_mac)


def parse_mac_address(mac_address):
    return [int(b, 16) for b in mac_address.split('-')]


def format_mac_address(bytes):
    return ':'.join(format(b, '02X') for b in bytes)


def format_mac_address_without_colon(mac_address):
    return mac_address.replace(":", "-")


def change_subflow_limit(config):
    subprocess.run(["ip", "mptcp", "limits", "set", "subflow", str(config["SubflowNr"])], shell=False)


def change_addr_accepted(config):
    subprocess.run(["ip", "mptcp", "limits", "set", "add_addr_accepted", str(config["AddAddrAcceptedNr"])], shell=False)


def change_dns(new_dns_server):
    # Specify the path to the resolv.conf file
    resolv_conf_path = "/etc/resolv.conf"

    # Create the new content with the updated DNS server
    new_content = f"nameserver {new_dns_server}\n"

    # Write the new content to the resolv.conf file
    with open(resolv_conf_path, 'w') as file:
        file.write(new_content)


def prioritize_ipv4():
    gai_conf_path = "/etc/gai.conf"
    new_line = "precedence ::ffff:0:0/96  100\n"
    with open(gai_conf_path, 'r') as file:
        existing_content = file.readlines()

    if any(line.strip() == new_line.strip() for line in existing_content):
        existing_content = [line.replace('#', '') if line.strip() == new_line.strip() else line for line in
                            existing_content]
    elif new_line not in existing_content:
        existing_content.append(new_line)
    with open(gai_conf_path, 'w') as file:
        file.writelines(existing_content)


def get_user_profile_path():
    command = 'sudo -u $(whoami) wslpath "$(/mnt/c/WINDOWS/System32/WindowsPowerShell/v1.0/powershell.exe \'$env:USERPROFILE\')"'
    result = subprocess.run(command, shell=True, stdout=subprocess.PIPE, text=True)
    return result.stdout.strip()


def get_config_path():
    user_profile_path = get_user_profile_path()
    path_delimiter = '/'
    return user_profile_path + path_delimiter + "AppData" + path_delimiter + "Local" + path_delimiter + "MPTCP" + path_delimiter + "config.json"


def get_interface_ip(iface):
    ipCommand = "ip -f inet addr show x | awk '/inet / {print $2}'".replace("x", iface)
    return \
        subprocess.run(ipCommand, shell=True, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True).stdout.replace(
            "\n", "").split("/")


def get_subnet(iface, mask):
    gw = get_default_prefix(iface)
    arr = list(gw)
    arr[-1] = "1"
    return gw + "/" + mask, ''.join(arr)


def assign_ip(iface):
    print(iface)
    subprocess.run(["dhclient", "-4", iface], shell=False)


def up_iface(iface):
    subprocess.run(["ip", "link", "set", "dev", iface, "up"])


def check_ipv4(iface):
    addresses = netifaces.ifaddresses(iface)
    if netifaces.AF_INET in addresses:
        return True
    else:
        return False


def assign_rule(ip):
    subprocess.run(["ip", "rule", "add", "from", ip, "table", str(table)])


def assign_route(iface, ip, gateway):
    global table
    global priority
    subprocess.run(["ip", "route", "add", ip, "dev", iface, "scope", "link", "table", str(table)])
    subprocess.run(["ip", "route", "add", "default", "via", gateway, "dev", iface, "table", str(table)])
    subprocess.run(["ip", "route", "add", "default", "via", gateway, "dev", iface, "metric", str(priority)])


def assign_endpoint(ip, config, iface):
    mac = get_mac_address(iface)
    type = get_entry_from_config(config, mac)['Types']
    param = []
    for str in type:
        if str == 'signal' or str == 'backup' or str == 'subflow':
            param.append(str)
    subprocess.run(["ip", "mptcp", "endpoint", "add", ip, "dev", iface] + param)


def get_mac_address(interface):
    try:
        # Get the MAC address of the specified network interface
        mac_address = netifaces.ifaddresses(interface)[netifaces.AF_LINK][0]['addr']
        return mac_address
    except KeyError:
        return None  # Return None if the MAC address is not available for the specified interface


def config_network(ifaceName, config):
    global table
    global priority
    up_iface(ifaceName)
    assign_ip(ifaceName)
    IPinformation = get_interface_ip(ifaceName)
    GWinformation = get_subnet(ifaceName, IPinformation[1])
    ifaceIP = IPinformation[0]
    ifaceGW = GWinformation[1]
    ifaceSN = GWinformation[0]
    assign_endpoint(ifaceIP, config, ifaceName)
    assign_rule(ifaceIP)
    assign_route(ifaceName, ifaceSN, ifaceGW)
    table += 1
    priority += 1


def handle_new_network_interface(interface_name, config):
    config_network(interface_name, config)


def main():
    context = pyudev.Context()
    monitor = pyudev.Monitor.from_netlink(context)
    monitor.filter_by(subsystem='net')
    config = read_config()
    print(config)
    if config is None or not config["ManageNetworkConfiguration"]:
        print("None")
    apply_general_config(config)
    for device in iter(monitor.poll, None):
        if device.action == 'add':
            handle_new_network_interface(device.sys_name, config)

def get_default_prefix(interface):
    routes = []
    with open('/proc/net/route') as f:
        for line in f:
            fields = line.strip().split()
            if fields[0] == interface and fields[1] != '00000000':
                destination_hex = fields[1]
                destination_ip = '.'.join(reversed([str(int(destination_hex[i:i+2], 16)) for i in range(0, 8, 2)]))
                routes.append(destination_ip)
    return routes[0]


if __name__ == "__main__":
    main()

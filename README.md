
# MPTCP Enabler

*This project aims to enable MPTCP on WSL2 by changing the WSL kernel with a MPTCP compatible kernel and by exposing all Windows Network Interfaces to WSL2.*

With this application u will be able to :

1. use all Windows NIC's on WSL2 with proper configuration
2. use a MPTCP-Proxy running on WSL2.
3. or even to route all the Windows traffic into WSL.
4. use a second proxy to make non compatible distant server compatible.


## Requirements
To run properly this application need to have :

- Windows 10/11 x86 Pro Version
- Windows Subsystem Linux enabled and updated
- Hyper-V enabled
- Have an Ubuntu distribution installed.

## Pre-installation setup
Windows will automatically turn off wifi when Ethernet is plugged in. If you want to try MPTCP over Wifi + Ethernet (or 4G through USB, all the same) you must disable this behavior :
1. Open Registry Editor.
2. Go to HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows\WcmSvc\Local.
3. Create/change the fMinimizeConnections registry DWORD to 0.
4. Close Registry Editor and reboot.

## Installation

### Step 1
Download the installer provider on the release sections of this repo and to run it as administator. it will install the MPTCP Enabler service that will be in charge of attaching all Network Interfaces into WSL2. At the end of the installation. u will able to see that the MPTCP Enabler Service is running.  
![MPTCP Service](https://i.ibb.co/JFnKSyY/Capture-d-cran-2024-03-03-161257.png)
### Step 2
Configure the service to be runned as a Windows user and not Local System. here's a [website](https://docs.microfocus.com/SM/9.61/Hybrid/Content/serversetup/tasks/configure_the_service_manager_service_to_run_as_a_windows_user.htm) that show how to do it.
### Step 3
Set the default distribution to Ubuntu by running : `wsl --set-default <Dist-Name>` and run `./install.sh` script on the Ubuntu distribution that will install script that will allow configure properly each network interface on WSL2, install the `mptcp-wsl.service` that will run a  proxy and redsocks to allow to redirect all the Windows traffic into a proxy.

## Configuration file Documentation
After the installation a config file named `config.json` should be created automatically and located at the  
`C:\Users\<USER>\AppData\Local\MPTCP\`folder and is structured as follow :

- **ManageKernelLocation** : If set to true, the application automatically put the correct path of the MPTCP Compatible Kernel located at : `C:\Program Files (x86)\MPTCP\MPTCP Enabler\bzImage`
- **ManageEndpoint** : if set to true, the endpoint settings will be applied into WSL.
- **KeepWSL2Awake**:if set to true, it will keep awake WSL2
- **Config**: Contains information about each NIC, endpoint parameters can be set on the Types section
- **Proxy**: this section is about using a distant MPTCP-Compatible Proxy. This settings will not be took into account if left blank.
- **SubflowNr**: This restricts the Multipath TCP connection to use up to  `n` different subflows. Servers should protect themselves by setting this limit to a few subflows. Most use cases would work well with 2 or 4 subflows.
- **AddAddrAcceptedNr**: This parameter limits the number of addresses that are learned over each Multipath TCP connection. This parameter could be used to protect the Multipath TCP implementation against attacks where two many addresses are advertised. Most use cases would work with 4 accepted addresses.

Each endpoints can be configured as follow:
-   `subflow`. When this flag is set, the path manager will try to create a subflow over this interface when a Multipath TCP is created or the interface becomes active while there was an ongoing Multipath TCP connection. This flag is mainly useful for clients.
-   `signal`. When this flag is set, the path manager will announce the address of the endpoint over any Multipath TCP connection created using other addresses. This flag can be used on clients or servers. It is mainly useful on servers that have multiple addresses.
-   `backup`. This flag can be combined with the two other flags. When combined with the  `subflow` flag, it indicates that a backup subflow will be created. When combined with the  `signal` flag, it indicates that the address will be advertised as a backup address.
## Appendix
- The "MPTCP Enabler" service will create for each NIC an HyperV switch that begin with "MPTCP -", this prefix is important and must not be changed.
- U could use MPTCP on other distribution like Debian or Kali or other since it will share the same kernel, the most important thing is that the `mptcp-wsl.service` is running on a Ubuntu distribution.
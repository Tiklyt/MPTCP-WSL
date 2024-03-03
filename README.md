


# MPTCP Enabler

*This project aims to enable MPTCP on WSL2 by changing the kernel with a MPTCP compatible kernel and by exposing all Windows Network Interfaces to WSL2.*

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
2.  Go to HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows\WcmSvc\Local.
3.  Create/change the fMinimizeConnections registry DWORD to 0.
4.  Close Registry Editor and reboot.

## Installation

### Step 1
Download the installer provider on the release sections of this repo and to run it as administator. it will install the MPTCP Enabler service that will be in charge of attaching all Network Interfaces into WSL2. At the end of the installation. u will able to see that the MPTCP Enabler Service is running.
![MPTCP Service](https://i.ibb.co/JFnKSyY/Capture-d-cran-2024-03-03-161257.png)
### Step 2
Configure the service to be runned as a Windows user and not Local System. here's a [website](https://docs.microfocus.com/SM/9.61/Hybrid/Content/serversetup/tasks/configure_the_service_manager_service_to_run_as_a_windows_user.htm) that show how to do it.
### Step 3
Run `./install.sh` script on your desired WSL2 distribution that will install script that will allow configure properly each network interface on WSL2, install the `mptcp-wsl.service` that will run a microsocks5 proxy and redsocks to allow to redirect all the Windows traffic into a proxy.

## Appendix

- U don't need to run the install.sh on every distribution, just on Ubuntuis sufficient since each distribution share the same kernel.
- The "MPTCP Enabler" service will create for each NIC an HyperV switch that begin with "MPTCP -", this prefix is important and must not be changed.
- U could use MPTCP on other distribution like Debian or Kali or other since it will share the same kernel, the most important thing is that the mptcp-wsl.service is running on a Ubuntu distribution.
 

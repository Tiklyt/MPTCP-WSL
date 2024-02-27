install:
	pip install -r requirements.txt
	sudo mkdir -p /usr/local/bin/mptcp
	sudo mv netconfig.py /usr/local/bin/mptcp/.
	sudo mv mptcp_netconfig.service /etc/systemd/system/.
	sudo systemctl enable mptcp_netconfig.service
	sudo systemctl start mptcp_netconfig.service

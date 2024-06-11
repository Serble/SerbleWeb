#!/bin/bash

# Define the location of your update script
UPDATE_SCRIPT="/usr/local/bin/updatedns.sh"

# Create the update script
cat << EOF > $UPDATE_SCRIPT
#!/bin/bash
DNS_IP=\$(dig @1.1.1.1 +short dns.serble.net | awk 'END{print}')
sudo sed -i "/^nameserver/c\nameserver \$DNS_IP # Updated on \$(date)" /etc/resolv.conf
EOF

# Make the update script executable
chmod +x $UPDATE_SCRIPT

# Define the location of your service file
SERVICE_FILE="/etc/systemd/system/update-dns-resolver.service"

# Create the service file
cat << EOF > $SERVICE_FILE
[Unit]
Description=Update /etc/resolv.conf with dns.serble.net's IP

[Service]
ExecStart=$UPDATE_SCRIPT

[Install]
WantedBy=multi-user.target
EOF

# Set the correct permissions for the service file
chmod 644 $SERVICE_FILE

# Reload the systemd daemon
systemctl daemon-reload

# Enable the service
systemctl enable update-dns-resolver

# Start the service
systemctl start update-dns-resolver

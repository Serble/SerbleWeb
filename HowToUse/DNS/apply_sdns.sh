#!/bin/bash
DNS_IP=$(dig @1.1.1.1 +short dns.serble.net | awk 'END{print}')
sudo sed -i "/^nameserver/c\nameserver $DNS_IP # $(date)" /etc/resolv.conf

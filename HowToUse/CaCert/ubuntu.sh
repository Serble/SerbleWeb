#!/bin/bash
wget -O SerbleRootCertificate.pem "https://files.serble.net/files/bk0qoj5skxl.pem?name=SerbleRootCertificate.pem" && sudo cp SerbleRootCertificate.pem /usr/local/share/ca-certificates/ && sudo update-ca-certificates

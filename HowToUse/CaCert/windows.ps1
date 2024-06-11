# Download .pem file
$url = "https://files.serble.net/files/bk0qoj5skxl.pem?name=SerbleRootCertificate.pem"
$pemOutput = "SerbleRootCertificate.pem"
Invoke-WebRequest -Uri $url -Outfile $pemOutput

# Convert .pem to .cer
$certOutput = "SerbleRootCertificate.cer"
openssl x509 -outform der -in $pemOutput -out $certOutput

# Import .cer into Certificate Store
Import-Certificate -FilePath $certOutput -CertStoreLocation Cert:\LocalMachine\Root

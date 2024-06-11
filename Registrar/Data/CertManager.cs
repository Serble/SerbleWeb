using System.Collections;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;

namespace Registrar.Data;

public static class CertManager {
    private static X509Certificate? _loadedCert;
    private static AsymmetricCipherKeyPair? _loadedKeyPair;

    private static X509Certificate RootCertificate {
        get {
            if (_loadedCert != null) {
                return _loadedCert;
            }
            
            // Load the cert
            using TextReader textReader = File.OpenText("rootca.pem");
            PemReader pemReader = new(textReader);
            _loadedCert = (X509Certificate)pemReader.ReadObject();
            return _loadedCert;
        }
    }

    private static AsymmetricCipherKeyPair RootKeyPair {
        get {
            if (_loadedKeyPair != null) {
                return _loadedKeyPair;
            }
            
            // Load the key pair
            using TextReader textReader = File.OpenText("rootca.key");
            PemReader pemReader = new(textReader);
            RsaPrivateCrtKeyParameters? parameters = (RsaPrivateCrtKeyParameters)pemReader.ReadObject();
            RsaKeyParameters pubParams = new(false, parameters.Modulus, parameters.PublicExponent);
            _loadedKeyPair = new AsymmetricCipherKeyPair(pubParams, parameters);
            return _loadedKeyPair;
        }
    }

    public static (X509Certificate, AsymmetricCipherKeyPair) GenerateSiteCert(params string[] domains) { 
        // Generate the Subject Keys
        RsaKeyPairGenerator rsaKeyPairGenerator = new();
        rsaKeyPairGenerator.Init(new KeyGenerationParameters(new SecureRandom(), 2048));
        AsymmetricCipherKeyPair subjectKeyPair = rsaKeyPairGenerator.GenerateKeyPair(); 

        // Generate the Subject Certificate
        X509V3CertificateGenerator certificateGenerator = new();
        BigInteger serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, new BigInteger(long.MaxValue.ToString()), new SecureRandom());
        certificateGenerator.SetSerialNumber(serialNumber);
        certificateGenerator.SetIssuerDN(RootCertificate.SubjectDN);
        certificateGenerator.SetSubjectDN(new X509Name("CN=" + domains[0]));
        certificateGenerator.SetPublicKey(subjectKeyPair.Public);
        certificateGenerator.SetNotBefore(DateTime.UtcNow.Date);
        certificateGenerator.SetNotAfter(DateTime.UtcNow.Date.AddYears(1));
        KeyUsage usage = new(KeyUsage.DigitalSignature | 
                              KeyUsage.NonRepudiation | 
                              KeyUsage.KeyEncipherment |
                              KeyUsage.DataEncipherment);
        certificateGenerator.AddExtension(X509Extensions.KeyUsage.Id, true, usage);
        
        // Create Subject Alternative Name
        List<GeneralName> generalNames = [];
        foreach(string domain in domains) {
            generalNames.Add(new GeneralName(GeneralName.DnsName, domain));
        }
        certificateGenerator.AddExtension(
            X509Extensions.SubjectAlternativeName.Id, false, 
            new DerSequence(generalNames.ToArray()));

        // Sign certificate 
        Asn1SignatureFactory signatureFactory = new("SHA256WITHRSA", RootKeyPair.Private);
        return (certificateGenerator.Generate(signatureFactory), subjectKeyPair); 
    }

    public static string ExportCertToString(X509Certificate cert) {
        using TextWriter textWriter = new StringWriter();
        PemWriter pemWriter = new(textWriter);
        pemWriter.WriteObject(cert);
        return textWriter.ToString()!;
    }
    
    public static string ExportKeyToString(AsymmetricCipherKeyPair keyPair) {
        StringWriter textWriter = new();
        PemWriter pemWriter = new(textWriter);
        pemWriter.WriteObject(keyPair.Private);
        return textWriter.ToString();
    }
}
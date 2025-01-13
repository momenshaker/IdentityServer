using App.Core.Enums;

namespace App.Core.Options;

public class OpenIdServerOptions
{
    public class CertificateOptions
    {
        public CertificateStoreType? StoreType { get; set; }
        public string CertificateName { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
    }

    public CertificateOptions EncryptionCertificate { get; set; } = new CertificateOptions();
    public CertificateOptions SigningCertificate { get; set; } = new CertificateOptions();
}
namespace App.Core.Options
{
    public class OpenIdServerOptions
    {
        public class CertificateOptions
        {
            public CertificateStoreType? StoreType { get; set; }
            public string CertificateName { get; set; }
            public string StoreName { get; set; }
        }

        public CertificateOptions EncryptionCertificate { get; set; }
        public CertificateOptions SigningCertificate { get; set; }
    }

    public enum CertificateStoreType
    {
        File,
        KeyVault
    }
 
}

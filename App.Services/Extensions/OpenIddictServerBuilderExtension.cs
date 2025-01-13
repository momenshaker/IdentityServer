using App.Core.Enums;
using App.Core.Options;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography.X509Certificates;
using static App.Core.Options.OpenIdServerOptions;

namespace App.Services.Extensions;

/// <summary>
/// Provides extension methods for configuring the OpenIddict server with certificates.
/// </summary>
public static class OpenIddictServerBuilderExtension
{
    /// <summary>
    /// Adds encryption and signing certificates to the OpenIddict server based on the provided configuration.
    /// </summary>
    /// <param name="builder">The OpenIddict server builder to which certificates will be added.</param>
    /// <param name="configuration">The configuration that contains the certificate settings.</param>
    /// <returns>The updated OpenIddict server builder with certificates configured.</returns>
    public static OpenIddictServerBuilder AddCertificates(this OpenIddictServerBuilder builder, IConfiguration configuration)
    {
        // Bind certificate options from the configuration
        var options = configuration.GetSection("OpenIdServer").Get<OpenIdServerOptions>();

        // Add encryption and signing certificates based on the configuration
        AddEncryptionCertificate(builder, options!.EncryptionCertificate);
        AddSigningCertificate(builder, options.SigningCertificate);

        return builder;
    }

    /// <summary>
    /// Adds the encryption certificate to the OpenIddict server based on the store type.
    /// </summary>
    /// <param name="builder">The OpenIddict server builder.</param>
    /// <param name="encryptionCertificate">The configuration for the encryption certificate.</param>
    private static void AddEncryptionCertificate(OpenIddictServerBuilder builder, CertificateOptions encryptionCertificate)
    {
        if (encryptionCertificate == null)
        {
            throw new ArgumentNullException(nameof(encryptionCertificate), "Encryption certificate configuration is missing.");
        }

        // Add certificate based on store type (KeyVault, File, or Development)
        switch (encryptionCertificate.StoreType)
        {
            case CertificateStoreType.KeyVault:
                builder.AddKeyVaultEncryptionCertificate(encryptionCertificate.CertificateName, encryptionCertificate.StoreName);
                break;

            case CertificateStoreType.File:
                builder.AddFileEncryptionCertificate(encryptionCertificate.CertificateName, encryptionCertificate.StoreName);
                break;

            default:
                builder.AddDevelopmentEncryptionCertificate();
                break;
        }
    }

    /// <summary>
    /// Adds the signing certificate to the OpenIddict server based on the store type.
    /// </summary>
    /// <param name="builder">The OpenIddict server builder.</param>
    /// <param name="signingCertificate">The configuration for the signing certificate.</param>
    private static void AddSigningCertificate(OpenIddictServerBuilder builder, CertificateOptions signingCertificate)
    {
        if (signingCertificate == null)
        {
            throw new ArgumentNullException(nameof(signingCertificate), "Signing certificate configuration is missing.");
        }

        // Add certificate based on store type (KeyVault, File, or Development)
        switch (signingCertificate.StoreType)
        {
            case CertificateStoreType.KeyVault:
                builder.AddKeyVaultSigningCertificate(signingCertificate.CertificateName, signingCertificate.StoreName);
                break;

            case CertificateStoreType.File:
                builder.AddFileSigningCertificate(signingCertificate.CertificateName, signingCertificate.StoreName);
                break;

            default:
                builder.AddDevelopmentSigningCertificate();
                break;
        }
    }

    /// <summary>
    /// Retrieves a certificate from Azure Key Vault.
    /// </summary>
    /// <param name="certificateName">The name of the certificate in Key Vault.</param>
    /// <param name="storeName">The name of the Key Vault store.</param>
    /// <returns>The downloaded X.509 certificate.</returns>
    private static X509Certificate2 GetKeyVaultCertificate(string certificateName, string storeName)
    {
        var client = new CertificateClient(new Uri($"https://{storeName}.vault.azure.net/"), new DefaultAzureCredential());
        return client.DownloadCertificate(certificateName).Value;
    }

    /// <summary>
    /// Retrieves a certificate from a local file system.
    /// </summary>
    /// <param name="certificateName">The name of the certificate file.</param>
    /// <param name="storeName">The name of the local directory where the certificate is stored.</param>
    /// <returns>The loaded X.509 certificate.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the certificate file cannot be found.</exception>
    private static X509Certificate2 GetFileCertificate(string certificateName, string storeName)
    {
        storeName ??= "Certificates"; // Default folder name if not specified
        string certificatePath = Path.Combine("App_Data", storeName, certificateName);

        if (!File.Exists(certificatePath))
        {
            throw new FileNotFoundException("Certificate file not found.", certificatePath);
        }

        return new X509Certificate2(certificatePath);
    }

    #region Certificate Store Extensions

    /// <summary>
    /// Adds a Key Vault encryption certificate to the OpenIddict server.
    /// </summary>
    /// <param name="builder">The OpenIddict server builder.</param>
    /// <param name="certificateName">The name of the encryption certificate.</param>
    /// <param name="storeName">The name of the Key Vault store.</param>
    /// <returns>The updated OpenIddict server builder with the encryption certificate.</returns>
    public static OpenIddictServerBuilder AddKeyVaultEncryptionCertificate(this OpenIddictServerBuilder builder, string certificateName, string storeName)
    {
        var certificate = GetKeyVaultCertificate(certificateName, storeName);
        return builder.AddEncryptionCertificate(certificate);
    }

    /// <summary>
    /// Adds a Key Vault signing certificate to the OpenIddict server.
    /// </summary>
    /// <param name="builder">The OpenIddict server builder.</param>
    /// <param name="certificateName">The name of the signing certificate.</param>
    /// <param name="storeName">The name of the Key Vault store.</param>
    /// <returns>The updated OpenIddict server builder with the signing certificate.</returns>
    public static OpenIddictServerBuilder AddKeyVaultSigningCertificate(this OpenIddictServerBuilder builder, string certificateName, string storeName)
    {
        var certificate = GetKeyVaultCertificate(certificateName, storeName);
        return builder.AddSigningCertificate(certificate);
    }

    /// <summary>
    /// Adds a file-based encryption certificate to the OpenIddict server.
    /// </summary>
    /// <param name="builder">The OpenIddict server builder.</param>
    /// <param name="certificateName">The name of the encryption certificate.</param>
    /// <param name="storeName">The name of the file-based store.</param>
    /// <returns>The updated OpenIddict server builder with the encryption certificate.</returns>
    public static OpenIddictServerBuilder AddFileEncryptionCertificate(this OpenIddictServerBuilder builder, string certificateName, string storeName)
    {
        var certificate = GetFileCertificate(certificateName, storeName);
        return builder.AddEncryptionCertificate(certificate);
    }

    /// <summary>
    /// Adds a file-based signing certificate to the OpenIddict server.
    /// </summary>
    /// <param name="builder">The OpenIddict server builder.</param>
    /// <param name="certificateName">The name of the signing certificate.</param>
    /// <param name="storeName">The name of the file-based store.</param>
    /// <returns>The updated OpenIddict server builder with the signing certificate.</returns>
    public static OpenIddictServerBuilder AddFileSigningCertificate(this OpenIddictServerBuilder builder, string certificateName, string storeName)
    {
        var certificate = GetFileCertificate(certificateName, storeName);
        return builder.AddSigningCertificate(certificate);
    }

    #endregion
}

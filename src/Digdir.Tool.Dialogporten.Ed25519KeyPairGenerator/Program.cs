using System.Diagnostics;
using NSec.Cryptography;

using var primaryKeyPair = Key.Create(SignatureAlgorithm.Ed25519, new KeyCreationParameters { ExportPolicy = KeyExportPolicies.AllowPlaintextExport });
var primaryPublicKey = primaryKeyPair.Export(KeyBlobFormat.RawPublicKey);
var primaryPrivateKey = primaryKeyPair.Export(KeyBlobFormat.RawPrivateKey);

using var secondaryKeyPair = Key.Create(SignatureAlgorithm.Ed25519, new KeyCreationParameters { ExportPolicy = KeyExportPolicies.AllowPlaintextExport });
var secondaryPublicKey = secondaryKeyPair.Export(KeyBlobFormat.RawPublicKey);
var secondaryPrivateKey = secondaryKeyPair.Export(KeyBlobFormat.RawPrivateKey);

var webApiProjectPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../Digdir.Domain.Dialogporten.WebApi"));

var keysValues = new Dictionary<string, string>
{
    { "Application:Dialogporten:Ed25519KeyPairs:Primary:Kid", "dev-primary-signing-key" },
    { "Application:Dialogporten:Ed25519KeyPairs:Primary:PublicComponent", Base64UrlEncode(primaryPublicKey) },
    { "Application:Dialogporten:Ed25519KeyPairs:Primary:PrivateComponent", Base64UrlEncode(primaryPrivateKey) },
    { "Application:Dialogporten:Ed25519KeyPairs:Secondary:Kid", "dev-secondary-signing-key" },
    { "Application:Dialogporten:Ed25519KeyPairs:Secondary:PublicComponent", Base64UrlEncode(secondaryPublicKey) },
    { "Application:Dialogporten:Ed25519KeyPairs:Secondary:PrivateComponent", Base64UrlEncode(secondaryPrivateKey) }
};

if (args.Contains("--set"))
{
    // Set the keys immediately
    foreach (var (key, value) in keysValues)
    {
        Process.Start("dotnet", $"user-secrets set -p {webApiProjectPath} \"{key}\" \"{value}\"")?.WaitForExit();
    }
}
else
{
    // Print the commands to set the keys
    Console.WriteLine("To set the keys as user secrets, run the following commands, or supply --set to run them automatically:");
    foreach (var (key, value) in keysValues)
    {
        Console.WriteLine($"dotnet user-secrets set -p {webApiProjectPath} \"{key}\" \"{value}\"");
    }
}

Console.WriteLine();
Console.WriteLine("For the keys to be used, set \"UseLocalDevelopmentCompactJwsGenerator\" to false in appsettings.Development.json");

static string Base64UrlEncode(byte[] input)
{
    return Convert.ToBase64String(input).Replace("+", "-").Replace("/", "_").TrimEnd('=');
}

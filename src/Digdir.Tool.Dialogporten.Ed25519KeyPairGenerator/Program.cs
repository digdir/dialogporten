using NSec.Cryptography;

using var primaryKeyPair = Key.Create(SignatureAlgorithm.Ed25519, new KeyCreationParameters { ExportPolicy = KeyExportPolicies.AllowPlaintextExport });
var primaryPublicKey = primaryKeyPair.Export(KeyBlobFormat.RawPublicKey);
var primaryPrivateKey = primaryKeyPair.Export(KeyBlobFormat.RawPrivateKey);

using var secondaryKeyPair = Key.Create(SignatureAlgorithm.Ed25519, new KeyCreationParameters { ExportPolicy = KeyExportPolicies.AllowPlaintextExport });
var secondaryPublicKey = secondaryKeyPair.Export(KeyBlobFormat.RawPublicKey);
var secondaryPrivateKey = secondaryKeyPair.Export(KeyBlobFormat.RawPrivateKey);

Console.WriteLine("Navigate to src/Digdir.Domain.Dialogporten.Application and run the following:");
Console.WriteLine();
Console.WriteLine("dotnet user-secrets set \"Application:Dialogporten:Ed25519KeyPairs:Primary:Kid\" \"dev-primary-signing-key\"");
Console.WriteLine("dotnet user-secrets set \"Application:Dialogporten:Ed25519KeyPairs:Primary:PublicComponent\" \"" + Base64UrlEncode(primaryPublicKey) + "\"");
Console.WriteLine("dotnet user-secrets set \"Application:Dialogporten:Ed25519KeyPairs:Primary:PrivateComponent\" \"" + Base64UrlEncode(primaryPrivateKey) + "\"");
Console.WriteLine("dotnet user-secrets set \"Application:Dialogporten:Ed25519KeyPairs:Secondary:Kid\" \"dev-secondary-signing-key\"");
Console.WriteLine("dotnet user-secrets set \"Application:Dialogporten:Ed25519KeyPairs:Secondary:PublicComponent\" \"" + Base64UrlEncode(secondaryPublicKey) + "\"");
Console.WriteLine("dotnet user-secrets set \"Application:Dialogporten:Ed25519KeyPairs:Secondary:PrivateComponent\" \"" + Base64UrlEncode(secondaryPrivateKey) + "\"");
Console.WriteLine();
Console.WriteLine("For the keys to be used, set \"UseLocalDevelopmentCompactJwsGenerator\" to false in appsettings.Development.json");

static string Base64UrlEncode(byte[] input)
{
    return Convert.ToBase64String(input).Replace("+", "-").Replace("/", "_").TrimEnd('=');
}

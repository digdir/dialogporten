using System.Buffers.Text;

namespace Digdir.Library.Dialogporten.WebApiClient.Services;

public static class Base64Url
{
    public static int GetMaxEncodedToUtf8Length(int length) => (length + 2) / 3 * 4;

    public static void Encode(ReadOnlySpan<byte> data, Span<byte> destination, out int bytesWritten)
    {
        Base64.EncodeToUtf8(data, destination, out _, out bytesWritten);
        for (var i = 0; i < bytesWritten; i++)
        {
            destination[i] = destination[i] switch
            {
                (byte)'+' => (byte)'-',
                (byte)'/' => (byte)'_',
                _ => destination[i]
            };
        }

        while (bytesWritten > 0 && destination[bytesWritten - 1] == '=')
        {
            bytesWritten--;
        }
    }

    public static byte[] Decode(string input)
    {
        var output = input.Replace('-', '+').Replace('_', '/');
        switch (output.Length % 4)
        {
            case 0: break;
            case 2:
                output += "==";
                break;
            case 3:
                output += "=";
                break;
            default:
                throw new ArgumentException("Illegal base64url string", nameof(input));
        }
        return Convert.FromBase64String(output);
    }
}

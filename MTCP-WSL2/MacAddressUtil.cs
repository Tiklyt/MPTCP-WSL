using System.Globalization;
using System.Management;

public class MacAddressUtil
{
    private static readonly byte[] key = { 0x42, 0x37, 0x29, 0x15, 0x5A, 0x3C };

    public static string Transform(string originalMac)
    {
        byte[] originalBytes = ParseMacAddress(originalMac);
        byte[] transformedBytes = new byte[originalBytes.Length];

        for (int i = 0; i < originalBytes.Length; i++)
        {
            transformedBytes[i] = (byte)(originalBytes[i] ^ key[i]);
        }

        return FormatMacAddress(transformedBytes);
    }

    public static string Revert(string transformedMac)
    {
        return Transform(transformedMac);
    }

    private static byte[] ParseMacAddress(string macAddress)
    {
        return macAddress.Split('-').Select(b => byte.Parse(b, NumberStyles.HexNumber)).ToArray();
    }

    private static string FormatMacAddress(byte[] bytes)
    {
        return BitConverter.ToString(bytes).Replace(":", "-");
    }

    public static string formatMacAddress(string macAddress)
    {
        return macAddress.Replace(":", "-");
    }
    
}
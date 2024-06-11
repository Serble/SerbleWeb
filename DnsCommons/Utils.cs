using System.Net;
using System.Text;

namespace DnsCommons;

public static class Utils {
    public const int DnsServerPort = 2856;
    public const int RecordSize = 5 + 128 + 256;
    public const char NullChar = '\0';

    public static byte[] EncodeRecord(this DnsRecord record) {
        byte[] type = Encoding.UTF8.GetBytes(record.Type);  // max 5 bytes
        byte[] name = Encoding.UTF8.GetBytes(record.Name);  // max 128 bytes
        byte[] value = Encoding.UTF8.GetBytes(record.Value);// max 256 bytes

        if (type.Length > 5 || name.Length > 128 || value.Length > 256) {
            throw new ArgumentException("Record parameters are too long", nameof(record));
        }
        
        byte[] final = new byte[RecordSize];
        
        Array.Copy(type, 0, final, 0, type.Length);
        Array.Copy(name, 0, final, 5, name.Length);
        Array.Copy(value, 0, final, 5 + 128, value.Length);

        return final;
    }

    public static DnsRecord DecodeRecord(this byte[] input) {
        byte[] type = new byte[5];
        byte[] name = new byte[128];
        byte[] value = new byte[256];

        Array.Copy(input, 0, type, 0, 5);
        Array.Copy(input, 5, name, 0, 128);
        Array.Copy(input, 5 + 128, value, 0, 256);

        return new DnsRecord(
            Encoding.UTF8.GetString(type).Replace(NullChar.ToString(), ""), 
            Encoding.UTF8.GetString(name).Replace(NullChar.ToString(), ""), 
            Encoding.UTF8.GetString(value).Replace(NullChar.ToString(), ""));
    }

    public static byte[] EncodeRecords(this DnsRecord[] records) {
        byte[][] encoded = records.Select(EncodeRecord).ToArray();
        byte[] final = new byte[encoded.Sum(arr => arr.Length)];
        
        int offset = 0;
        foreach (byte[] record in encoded) {
            Array.Copy(record, 0, final, offset, record.Length);
            offset += record.Length;
        }

        return final;
    }

    public static DnsRecord[] DecodeRecords(this byte[] input) {
        List<DnsRecord> records = [];
        int offset = 0;
        
        while (offset < input.Length) {
            byte[] record = new byte[5 + 128 + 256];
            Array.Copy(input, offset, record, 0, 5 + 128 + 256);
            records.Add(DecodeRecord(record));
            offset += 5 + 128 + 256;
        }

        return records.ToArray();
    }

    public static string ToReadableString(this DnsRecord[] records) {
        if (records.Length == 0) {
            return "None";
        }
        return string.Join("\n", records.Select(record => $"{record.Type} {record.Name} {record.Value}"));
    }

    public static T GetRandom<T>(this T[] arr) {
        return arr[Random.Shared.Next(0, arr.Length)];
    }

    public static string AsIpv4String(this IPAddress addr) {
        return addr.ToString().Replace("::ffff:", "");
    }
}
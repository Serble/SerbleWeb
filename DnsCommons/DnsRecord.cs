namespace DnsCommons;

/// <summary>
/// A DNS record.
/// </summary>
/// <param name="Type">What type the record is, can be:
/// - IP4
/// - IP6
/// - TXT
/// Max 5 characters.
/// </param>
/// <param name="Name">The fully qualified domain, max 128 characters.</param>
/// <param name="Value">The data of the record, max 256 characters.</param>
public record DnsRecord(string Type, string Name, string Value);

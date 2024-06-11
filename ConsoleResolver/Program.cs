/*
 * Console Resolver
 * ------------------------------
 * An example of a simple Serble DNS resolver
 * that can be used via the terminal.
 */

using System.Net;
using DnsCommons;

while (true) {
    Console.Write("Enter query: ");
    string query = Console.ReadLine()!;
    if (query[0] == '#') {  // IP Resolve
        IPAddress? address = await DnsResolver.DnsResolver.ResolveIp(query[1..]);
        Console.WriteLine($"Response ({query[1..]}):\n{address?.ToString() ?? "None"}");
    }
    else {
        DnsRecord[] records = await DnsResolver.DnsResolver.Query(query);
        Console.WriteLine($"Response:\n{records.ToReadableString()}");
    }
}
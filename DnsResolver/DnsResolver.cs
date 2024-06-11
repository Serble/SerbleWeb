using System.Net;
using System.Net.Sockets;
using System.Text;
using DnsCommons;

namespace DnsResolver;

/// <summary>
/// Resolves Serble domain names to IP addresses or their records
/// by contacting a Serble DNS server.
///
/// Note that the Serble DNS server will fall back to regular DNS if
/// it fails to find the queried domain. So this can be used as a full
/// replacement for System.Net.Dns.
/// </summary>
public static class DnsResolver {
    private const string SerbleDnsIp = "sdns.serble.net";  // This has to be resolved using regular DNS
    private static IPAddress? _dnsIp;  // Cache the IP of the above host

    private static IPAddress DnsIp {
        get {
            if (_dnsIp != null) {
                return _dnsIp;
            }
            IPAddress[] results = Dns.GetHostEntry(SerbleDnsIp).AddressList;
            _dnsIp = results[0];
            return _dnsIp;
        }
    }

    /// <summary>
    /// Queries all records for a hostname.
    /// </summary>
    /// <param name="name">The hostname to query/</param>
    /// <returns>An array of all records under that hostname.</returns>
    public static async Task<DnsRecord[]> Query(string name) {
        UdpClient client = new();
        IPEndPoint dnsServer = new(DnsIp, Utils.DnsServerPort);
        byte[] request = Encoding.UTF8.GetBytes(name);
        await client.SendAsync(request, request.Length, dnsServer);
        
        // Receive response
        UdpReceiveResult result = await client.ReceiveAsync();
        DnsRecord[] response = result.Buffer.DecodeRecords();
        return response;
    }

    /// <summary>
    /// Resolves a hostname to an IPAddress.
    /// 
    /// Resolves:
    /// - IP4 (A regular mapping to an IPv4 address)
    /// - ALIAS (A mapping to another domain)
    /// - RDNS (A mapping to a normal DNS domain, not Serble)
    /// </summary>
    /// <param name="name">The hostname to resolve.</param>
    /// <param name="rDnsFallback">
    /// Whether to query regular DNS upon no result being found, this isn't needed because
    /// the Serble DNS server does this for you. Enabling this will result in double checking
    /// for no reason.
    /// </param>
    /// <param name="ignoreError">Whether to return null instead of throwing format exception upon invalid IP4 record.</param>
    /// <returns>The resolved IPAddress or null if not result was found.</returns>
    /// <exception cref="FormatException">The DNS responded with an invalid IP4 record, you should be handling this or use ignoreError.</exception>
    public static async Task<IPAddress?> ResolveIp(string name, bool rDnsFallback = false, bool ignoreError = true) {
        DnsRecord[] response = await Query(name);

        DnsRecord[] ip4 = response.Where(r => r.Type == "IP4").ToArray();
        if (ip4.Length != 0) {
            string randomValue = ip4.GetRandom().Value;
            try {
                return IPAddress.Parse(randomValue);
            }
            catch (FormatException) {
                if (ignoreError) {
                    return null;
                }
                throw new FormatException("Could not resolve: DNS provided invalid IP " + randomValue);
            }
        }
        
        DnsRecord[] alias = response.Where(r => r.Type == "ALIAS").ToArray();
        if (alias.Length != 0) {
            return await ResolveIp(alias.GetRandom().Value);
        }
        
        DnsRecord[] rdns = response.Where(r => r.Type == "RDNS").ToArray();
        if (rdns.Length != 0) {
            return (await Dns.GetHostEntryAsync(rdns.GetRandom().Value)).AddressList.GetRandom();
        }

        if (rDnsFallback) {
            IPAddress[] result = (await Dns.GetHostEntryAsync(name)).AddressList;
            return result.Length == 0 ? null : result.GetRandom();
        }
        
        return null;
    }
}
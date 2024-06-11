using System.Collections.Concurrent;
using System.Net;
using DnsClient;
using DnsCommons;
using GeneralPurposeLib;

namespace DomainNameServer;

public static class Resolver {
    private const ulong MaxCacheAge = 600;  // 1 hour
    private static readonly ConcurrentDictionary<string, DnsRecord[]> Cache = new();
    private static readonly ConcurrentDictionary<string, ulong> CacheAges = new();
    private static IPEndPoint? _realDnsIp;
    
    private static IPEndPoint RealDnsIp {
        get {
            if (_realDnsIp != null) {
                return _realDnsIp;
            }

            _realDnsIp = new IPEndPoint(IPAddress.Parse(GlobalConfig.Config["realdns_ip"].Text), GlobalConfig.Config["realdns_port"].Integer);
            return _realDnsIp;
        }
    }

    private static ulong GetTimestamp() {
        return (ulong) DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
    
    private static ulong GetExpireTime() {
        return (ulong) DateTimeOffset.UtcNow.ToUnixTimeSeconds() + MaxCacheAge;
    }
    
    public static Task<DnsRecord[]> Resolve(string domain, params string[] types) {
        return Resolve(domain, true, types);
    }
    
    public static async Task<DnsRecord[]> Resolve(string domain, bool cache = true, params string[] types) {
        if (cache && Cache.TryGetValue(domain, out DnsRecord[]? value) && GetTimestamp() < CacheAges[domain]) {
            return value;
        }

        DnsRecord[] records = await ResolveNoCache(domain, types);
        Cache[domain] = records;
        CacheAges[domain] = GetExpireTime();
        return records;
    }
    
    private static async Task<DnsRecord[]> ResolveNoCache(string domain, params string[] types) {
        // Trim trailing spaces, periods
        domain = domain.Trim().TrimEnd('.');
        
        DnsRecord[] resolve = await Program.Storage.GetRecordsAsync(domain.ToLower());

        if (resolve.Length == 0) {
            LookupClient lookupClient = new(RealDnsIp);
            IDnsQueryResponse? result = await lookupClient.QueryAsync(domain, QueryType.A);
            resolve = result.Answers.ARecords().Select(a => new DnsRecord("IP4", domain, a.Address.AsIpv4String())).ToArray();
        }

        if (types.Length == 0) {
            return resolve;
        }

        return resolve.Where(record => types.Contains(record.Type)).ToArray();
    }
}
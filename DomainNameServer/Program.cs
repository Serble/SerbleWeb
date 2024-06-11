using DnsCommons;
using GeneralPurposeLib;

namespace DomainNameServer;

internal static class Program {
    internal static readonly DnsServerStorage Storage = new();
    
    private static readonly Dictionary<string, Property> DefaultConfig = new() {
        { "mysql_host", "mysql.serble.net" },
        { "mysql_user", "admin" },
        { "mysql_password", "secret password" },
        { "mysql_database", "dns" },
        { "realdns_ip", "1.1.1.1" },
        { "realdns_port", 53 }
    };
    
    public static void Main(string[] args) {
        Logger.Init(LogLevel.Debug);

        Config config = new(DefaultConfig);
        GlobalConfig.Init(config);
        
        Storage.Init(config["mysql_host"], config["mysql_user"], config["mysql_password"], config["mysql_database"]);
        Server.Start();
        Thread.Sleep(-1);
    }
}
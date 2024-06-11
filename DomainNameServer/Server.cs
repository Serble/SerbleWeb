using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using DnsCommons;
using GeneralPurposeLib;
using Utils = DnsCommons.Utils;

namespace DomainNameServer;

public class Server {
    private static UdpClient _listener;
    private static Thread _listenerThread;
    private static long _responses;
    // private static ConcurrentBag<Thread> _handlerThreads = [];

    public static void Start() {
        _listener = new UdpClient(Utils.DnsServerPort);
        _listenerThread = new Thread(BeginListening);
        
        _listenerThread.Start();
    }

    private static async void BeginListening() {
        while (true) {
            UdpReceiveResult result = await _listener.ReceiveAsync();
            Thread handler = new(async () => {
                try {
                    await HandleRequest(result);
                }
                catch (Exception e) {
                    Logger.Error(e);
                }
            });
            handler.Start();
            // _handlerThreads.Add(handler);
        }
    }

    private static async Task HandleRequest(UdpReceiveResult result) {
        IPEndPoint sender = result.RemoteEndPoint;

        string name = Encoding.UTF8.GetString(result.Buffer);
        DnsRecord[] records = await Resolver.Resolve(name);
        
        byte[] output = records.EncodeRecords();

        await _listener.SendAsync(output, output.Length, sender);
        _responses++;
        Logger.Debug($"[{_responses}] Responded to DNS query from: {sender}");
    }
}
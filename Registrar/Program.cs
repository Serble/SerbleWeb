using DnsCommons;
using GeneralPurposeLib;
using Registrar.Components;
using LogLevel = GeneralPurposeLib.LogLevel;

namespace Registrar;

internal static class Program {
    public static SerbleApi.SerbleApi Api = null!;
    public static DnsServerStorage Storage = null!;
    private static readonly Dictionary<string, Property> DefaultConfig = new() {
        { "app-id", "73eed23d-8653-4cf9-89fd-560ac025146b" },
        { "app-secret", "Some secret" },
        { "app-scope", "user_info" },
        { "mysql_host", "mysql.serble.net" },
        { "mysql_user", "admin" },
        { "mysql_password", "secret password" },
        { "mysql_database", "dns" }
    };
    
    public static void Main(string[] args) {
        Logger.Init(LogLevel.Debug);
        Config config = new(DefaultConfig);
        GlobalConfig.Init(config);
        
        Api = new SerbleApi.SerbleApi(config["app-id"], config["app-secret"]);
        Storage = new DnsServerStorage();
        Storage.Init(config["mysql_host"], config["mysql_user"], config["mysql_password"], config["mysql_database"]);
        
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment()) {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
        }

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}
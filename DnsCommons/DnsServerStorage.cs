using MySql.Data.MySqlClient;

namespace DnsCommons;

public class DnsServerStorage {
    private string _connectString = "";

    public void Init(string host, string user, string password, string database) {
        _connectString = $"server={host};" +
                         $"userid={user};" +
                         $"password={password};" +
                         $"database={database};" +
                         $"Max Pool Size=200;";
        UpdateTables();
    }

    public void Deinit() {
        
    }
    
    public void UpdateTables() {
        SendMySqlStatement(@"CREATE TABLE IF NOT EXISTS records(
                           name VARCHAR(256),
                           type VARCHAR(5),
                           value VARCHAR(256),
                           created_at DATETIME DEFAULT CURRENT_TIMESTAMP)");
        SendMySqlStatement(@"CREATE TABLE IF NOT EXISTS ownership(
                           namespace VARCHAR(256),
                           ownerid VARCHAR(256),
                           created_at DATETIME DEFAULT CURRENT_TIMESTAMP)");
    }

    private async Task<MySqlConnection> GetOpenConnection() {
        MySqlConnection con = new(_connectString);
        await con.OpenAsync();
        return con;
    }

    private async Task<T> ReadData<T>(string query, Func<MySqlDataReader, T> action) {
        MySqlConnection con = await GetOpenConnection();
        MySqlCommand command = new(query, con);
        MySqlDataReader table = command.ExecuteReader();
        T result = action.Invoke(table);
        await con.CloseAsync();
        return result;
    }

    private void SendMySqlStatement(string statement) {
        MySqlHelper.ExecuteNonQuery(_connectString, statement);
    }
    
    private async Task SendMySqlStatementAsync(string statement) {
        MySqlConnection con = await GetOpenConnection();
        MySqlCommand command = new(statement, con);
        await command.ExecuteNonQueryAsync();
        await con.CloseAsync();
    }
    
    public async Task AddRecordAsync(DnsRecord record) {
        await SendMySqlStatementAsync($"INSERT INTO records (name, type, value) VALUES ('{record.Name}', '{record.Type}', '{record.Value}')");
    }
    
    public async Task AddOwnershipAsync(string @namespace, string ownerid) {
        await SendMySqlStatementAsync($"INSERT INTO ownership (namespace, ownerid) VALUES ('{@namespace}', '{ownerid}')");
    }
    
    public async Task RemoveRecordAsync(string name) {
        await SendMySqlStatementAsync($"DELETE FROM records WHERE name='{name}'");
    }
    
    public async Task RemoveOwnershipAsync(string @namespace, string ownerid) {
        await SendMySqlStatementAsync($"DELETE FROM ownership WHERE namespace='{@namespace}' AND ownerid='{ownerid}'");
    }

    public async Task<DnsRecord[]> GetRecordsAsync(string name) {
        return await ReadData<DnsRecord[]>($"SELECT * FROM records WHERE name='{name}'", table => {
            List<DnsRecord> records = [];
            while (table.Read()) {
                records.Add(new DnsRecord(table.GetString("type"), table.GetString("name"), table.GetString("value")));
            }
            return records.ToArray();
        });
    }

    public async Task<string[]> GetUsersNamespaces(string userId) {
        return await ReadData<string[]>($"SELECT * FROM ownership WHERE ownerid='{userId}'", table => {
            List<string> namespaces = [];
            while (table.Read()) {
                namespaces.Add(table.GetString("namespace"));
            }
            return namespaces.ToArray();
        });
    }

    /// <summary>
    /// Gets all records where the name ends in the provided namespace.
    /// </summary>
    /// <param name="space">The namespace to match</param>
    /// <returns>A list of the found records.</returns>
    public async Task<DnsRecord[]> GetNamespaceRecords(string space) {
        return await ReadData($"SELECT * FROM records WHERE name LIKE '%.{space}'", table => {
            List<DnsRecord> records = [];
            while (table.Read()) {
                records.Add(new DnsRecord(table.GetString("type"), table.GetString("name"), table.GetString("value")));
            }
            return records.ToArray();
        });
    }

    public async Task<string?> GetOwnerOfNamespace(string space) {
        return await ReadData($"SELECT * FROM ownership WHERE namespace='{space}'", table => {
            return table.Read() ? table.GetString("ownerid") : null;
        });
    }
    
}
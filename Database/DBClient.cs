
using Microsoft.Data.SqlClient;
using System.Data;
using static WebsiteComputer.Models.ClientDtos;
namespace WebsiteComputer.Database
{
    internal class DBClient
    {

        //public static async Task Main(string[] args)
        //{
        //    var config = new ConfigurationBuilder()
        //   .SetBasePath(Directory.GetCurrentDirectory())
        //   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        //   .Build();
        //    var connStr = config.GetConnectionString("Default")
        //        ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
        //    //var clientNew = new ClientDetail
        //    //{
        //    //    accountID = 0,
        //    //    clientName = "Duong duy hoang",
        //    //    clientAddress = "Quang tri",
        //    //    phoneNumber = "1234567890",
        //    //    clientCode = "aaa",
        //    //    totalMoney = 0,
        //    //    username = "clientaaa",
        //    //    password = "1"
        //    //};
        //    //await deleteClient(connStr, "CLI-0010");
        //    //var client = await CreateClient(connStr, clientNew);
        //    //var List = await readListClient(connStr);
        //    //var json = JsonSerializer.Serialize(List, new JsonSerializerOptions
        //    //{
        //    //    WriteIndented = true,
        //    //    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        //    //    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        //    //});
        //    //Console.OutputEncoding = System.Text.Encoding.UTF8;
        //    //Console.WriteLine(json);
        //    //await UpdateClient(connStr, "CLI-0009", "Trần Thị Duyên", "0345248654");

        //    ClientInformation client = new ClientInformation();
        //    ClientLogin info = new ClientLogin("client3", "1");
        //    client = await Login(connStr, info);
        //    var json = JsonSerializer.Serialize(client, new JsonSerializerOptions
        //    {
        //        WriteIndented = true,
        //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        //        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        //    });
        //    Console.WriteLine(json);
        //}
        public static async Task<List<ClientDetail>> readListClient(string connStr)
            => await GetClientList(connStr);
        public static async Task<List<ClientDetail>> GetClientList(string connStr)
        {
            var listClient = new List<ClientDetail>();
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"
                SELECT 
	                c.AccountID		  AS AccountID,
                    c.ClientCode      AS clientCode,
                    c.ClientName      AS clientName,
                    c.PhoneNumber     AS phoneNumber,
                    c.ClientAddress   AS clientAddress,
                    COALESCE(SUM(o.TotalPrice), 0) AS totalMoney
	
                FROM dbo.Client AS c
                LEFT JOIN dbo.[Orders] AS o
                    ON o.ClientID = c.ClientID
                GROUP BY 
	                c.AccountID,
                    c.ClientCode,
                    c.ClientName,
                    c.PhoneNumber,
                    c.ClientAddress;";
                using var cmd = new SqlCommand(sql, conn);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    listClient.Add(new ClientDetail
                    {
                        accountID = reader.GetInt32(reader.GetOrdinal("AccountID")),
                        clientCode = reader.GetString(reader.GetOrdinal("clientCode")),
                        clientName = reader.GetString(reader.GetOrdinal("clientName")),
                        phoneNumber = reader.GetString(reader.GetOrdinal("phoneNumber")),
                        clientAddress = reader.GetString(reader.GetOrdinal("clientAddress")),
                        totalMoney = reader.GetDecimal(reader.GetOrdinal("totalMoney"))
                    });
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return listClient;
        }

        public static async Task<int> CreateClient(string connStr, ClientDetail client)
        {
            int clientID = 0;
            
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                await using var tx = await conn.BeginTransactionAsync();
                var now = DateTime.UtcNow;
                var ClientCode = $"CLI-{now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";

                var AccCode = $"Acc-{now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
                var sql = @"
                            declare 
                             @ClientID int


                            INSERT INTO [dbo].[Account]
                                       ([AccountCode]
                                       ,[Username]
                                       ,[PasswordHash]
                                       ,[Roles])
                            output inserted.AccountID
                            VALUES
                                (@AccountCode
                                ,@Username
                                ,@PasswordHash
                                ,@Roles)

                            Declare @AccountID Int = cast(scope_identity() as int);
                            INSERT INTO [dbo].[Client]
                                       ([AccountID] 
                                       ,[ClientCode]
                                       ,[ClientName]
                                       ,[PhoneNumber]
                                       ,[ClientAddress]
                                       ,[TotalMoney])
                                 VALUES
                                       (@AccountID
                                       ,@ClientCode
                                       ,@ClientName
                                       ,@PhoneNumber
                                       ,@ClientAddress
                                       ,@TotalMonney)

                            select @ClientID = ClientID 
                            from Dbo.Client as cl
                            where cl.ClientCode = @ClientCode

                            INSERT INTO [dbo].[Cart]
                                       ([ClientID])
                                 VALUES
                                       (@ClientID)
                                
                            ";
                using var cmd = new SqlCommand(sql, conn, (SqlTransaction)tx);
                cmd.Parameters.Add(new SqlParameter("@AccountCode", SqlDbType.VarChar) { Value = AccCode });
                cmd.Parameters.Add(new SqlParameter("@ClientCode", SqlDbType.VarChar) { Value = ClientCode });
                cmd.Parameters.Add(new SqlParameter("@Username", SqlDbType.VarChar) { Value = client.username });
                cmd.Parameters.Add(new SqlParameter("@PasswordHash", SqlDbType.VarChar) { Value = client.password });
                cmd.Parameters.Add(new SqlParameter("@ClientName", SqlDbType.NVarChar) { Value = client.clientName });
                cmd.Parameters.Add(new SqlParameter("@PhoneNumber", SqlDbType.VarChar) { Value = client.phoneNumber });
                cmd.Parameters.Add(new SqlParameter("@ClientAddress", SqlDbType.NVarChar) { Value = client.clientAddress });
                cmd.Parameters.Add(new SqlParameter("@Roles", SqlDbType.VarChar) { Value = "client" });
                var clientIDobj = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                clientID = Convert.ToInt32(clientIDobj);
                await tx.CommitAsync().ConfigureAwait(false);
               
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return clientID;
        }
        public static async Task<int> UpdateClient(string connStr, string clientCode, string ClientName, string PhoneNumber )
        {
            int clientID = await ConnectDB.GetClientIDFromClientCode(connStr, clientCode);

            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                await using var tx = await conn.BeginTransactionAsync();
                var sql = @"
                            UPDATE [dbo].[Client]
                               SET 
                                   [ClientName] = @ClientName
                                  ,[PhoneNumber] = @PhoneNumber
                             WHERE ClientCode = @ClientCode 
                                                        ";
                using var cmd = new SqlCommand(sql, conn, (SqlTransaction)tx);
                cmd.Parameters.Add(new SqlParameter("@ClientName", SqlDbType.NVarChar) { Value = ClientName });
                cmd.Parameters.Add(new SqlParameter("@PhoneNumber", SqlDbType.VarChar) { Value = PhoneNumber });
                cmd.Parameters.Add(new SqlParameter("@ClientCode", SqlDbType.VarChar) { Value = clientCode });

                var clientIDobj = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                clientID = Convert.ToInt32(clientIDobj);
                await tx.CommitAsync().ConfigureAwait(false);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return clientID;
        }

        public static async Task deleteClient(string connStr, string clientCode)
        {
            int ClientID = await ConnectDB.GetClientIDFromClientCode(connStr, clientCode);
            try
            {

                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
               
                var sql = @"
                            BEGIN TRY
                                BEGIN TRAN;

                                -- Replace with your key or filter (ClientID or ClientCode)
                                DECLARE @ClientID INT ;
	                            select cl.AccountID from Client as cl where cl.ClientID = @ClientID

	                            delete Account where AccountID= ( select cl.AccountID from client as cl where ClientID = @ClientID)
                                -- 1) Delete dependent rows in child tables
                                DELETE FROM dbo.Cart WHERE ClientID = @ClientID;

                                -- If you also have Orders -> OrderItems, handle in correct order:
                                DELETE oi
                                FROM dbo.OrderItems oi
                                JOIN dbo.Orders o ON o.OrderID = oi.OrderID
                                WHERE o.ClientID = @ClientID;

                                DELETE FROM dbo.Orders WHERE ClientID = @ClientID;

                                -- 2) Delete the client
                                DELETE FROM dbo.Client WHERE ClientID = @ClientID;
	                            DELETE FROM [dbo].[Cart]
                                  WHERE Cart.ClientID = @ClientID
                                COMMIT TRAN;
                            END TRY
                            BEGIN CATCH
                                IF @@TRANCOUNT > 0 ROLLBACK TRAN;
                                THROW;
                            END CATCH;";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@ClientID", SqlDbType.Int) { Value = ClientID });
                await cmd.ExecuteNonQueryAsync();
                Console.WriteLine("dddddd");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }
        public static async Task<ClientInformation> Login(string conStr, ClientLogin clientLogin ) {
            var client = new ClientInformation();
            try {
                var conn = ConnectDB.Create(conStr);
                await conn.OpenAsync();
                var sql = @"
                            SELECT [ClientCode] as clientCode
                                  ,[ClientName] as clientName
                                  ,[PhoneNumber] as phoneNumber
                                  ,[ClientAddress] as clientAddress 
                                  ,[TotalMoney] as totalMoney
	  
                              FROM [dbo].[Client] as c
  
                              left join dbo.Account as a
                              on a.AccountID = c.AccountID
                              where a.Username = @username and a.PasswordHash = @password
                            ";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@username", SqlDbType.VarChar) { Value = clientLogin.username });
                cmd.Parameters.Add(new SqlParameter("@password", SqlDbType.VarChar) { Value = clientLogin.password });
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    client.clientName = reader.GetString(reader.GetOrdinal("clientName"));
                    client.clientCode = reader.GetString(reader.GetOrdinal("clientCode"));
                    client.phoneNumber = reader.GetString(reader.GetOrdinal("phoneNumber"));
                    client.clientAddress = reader.GetString(reader.GetOrdinal("clientAddress"));
                    client.totalMoney = reader.GetDecimal(reader.GetOrdinal("totalMoney"));

                }
            }
            catch
            {
                throw;
            }
            return client;
        }
    }
}

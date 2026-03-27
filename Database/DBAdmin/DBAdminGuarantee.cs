using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection.PortableExecutable;
using System.Text;
using WebsiteComputer.Database;
using WebsiteComputer.Models.Policy;
using static WebsiteComputer.Models.Policy.Guarantee;

namespace Database.DBAdmin
{
    public class DBAdminGuarantee
    {
        public static async Task<GuaranteeProduct> CreateGuarantee(string connStr, GuaranteeProduct guranteeInfo){
            try {
                var now = DateTime.UtcNow;
                guranteeInfo.id  = $"GUA-{now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("", SqlDbType.VarChar){ Value = guranteeInfo.id});
                cmd.Parameters.Add(new SqlParameter("", SqlDbType.VarChar){ Value = guranteeInfo.productID});
                cmd.Parameters.Add(new SqlParameter("", SqlDbType.DateTime2){ Value = guranteeInfo.dateStart});
                cmd.Parameters.Add(new SqlParameter("", SqlDbType.DateTime2){ Value = guranteeInfo.dateEnd});
                await cmd.ExecuteNonQueryAsync();

            }
            catch {
                throw ;
            }
            return guranteeInfo;
        }
        public static async Task<GuaranteeProduct> ReadGuarantee(string connStr, string guaranteeCode){
            var guranteeInfo = new GuaranteeProduct();
            try {
                var now = DateTime.UtcNow;
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("", SqlDbType.VarChar){ Value = guaranteeCode});
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    guranteeInfo.id = reader.GetString(reader.GetOrdinal(""));
                    guranteeInfo.productID = reader.GetString(reader.GetOrdinal(""));
                    guranteeInfo.dateStart = reader.GetDateTime(reader.GetOrdinal(""));
                    guranteeInfo.dateEnd = reader.GetDateTime(reader.GetOrdinal(""));
                }
            }
            catch {
                throw ;
            }
            return guranteeInfo;
        }
        public static async Task<List<GuaranteeProduct>> ReadListGuarantee(string connStr){
            var listGuranteeInfo = new List<GuaranteeProduct>();
            GuaranteeProduct guranteeInfo;
            try {
                var now = DateTime.UtcNow;
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"";
                await using var cmd = new SqlCommand(sql, conn);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    listGuranteeInfo.Add( guranteeInfo = new GuaranteeProduct()
                        {
                            id = reader.GetString(reader.GetOrdinal("")),
                            productID = reader.GetString(reader.GetOrdinal("")),
                            dateStart = reader.GetDateTime(reader.GetOrdinal("")),
                            dateEnd = reader.GetDateTime(reader.GetOrdinal(""))
                        }
                    );
                }
            }
            catch {
                throw ;
            }
            return listGuranteeInfo;
        }
        public static async Task<GuaranteeProduct> UpdateGuarantee(string connStr, GuaranteeProduct guranteeInfo){
            try {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("", SqlDbType.VarChar){ Value = guranteeInfo.id});
                cmd.Parameters.Add(new SqlParameter("", SqlDbType.VarChar){ Value = guranteeInfo.productID});
                cmd.Parameters.Add(new SqlParameter("", SqlDbType.DateTime2){ Value = guranteeInfo.dateStart});
                cmd.Parameters.Add(new SqlParameter("", SqlDbType.DateTime2){ Value = guranteeInfo.dateEnd});
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    guranteeInfo.id = reader.GetString(reader.GetOrdinal(""));
                    guranteeInfo.productID = reader.GetString(reader.GetOrdinal(""));
                    guranteeInfo.dateStart = reader.GetDateTime(reader.GetOrdinal(""));
                    guranteeInfo.dateEnd = reader.GetDateTime(reader.GetOrdinal(""));
                }
            }
            catch {
                throw ;
            }
            return guranteeInfo;
        }
        public static async Task<string> DeleteGuarantee(string connStr, string id){
            try {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("", SqlDbType.VarChar){ Value = id});
                await cmd.ExecuteNonQueryAsync();
            }
            catch {
                throw ;
            }
            return id ;
        }
    }
}

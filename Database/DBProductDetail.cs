using WebsiteComputer.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Text.Encodings.Web;
using System.Text.Json;
using WebsiteComputer.Models;
using static WebsiteComputer.Models.AdminProduct;

namespace WebsiteComputer.Database
{
    public static class DBProductDetail
    {
        //public static async Task Main(string[] args)
        //{

        //    var builder = WebApplication.CreateBuilder(args);

        //    var config = new ConfigurationBuilder()
        //       .SetBasePath(Directory.GetCurrentDirectory())
        //       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        //       .Build();
        //    var connStr = config.GetConnectionString("Default")
        //        ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
        //    //var json = await ReadAsJsonAync(connStr, "P001");
        //    //Console.WriteLine(json);

        //}

        public static async Task<List<ProductSpec?>> GetProductSpecAsync(string connStr, string ProductCode)
        {
            var listSpec = new List<ProductSpec?>();
            int ProductID = await ConnectDB.GetProductIDFromProductCode(connStr, ProductCode);
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();

                var sql = @$"                  
                            SELECT [SpecKey] as speckey
                                  ,[SpecValue] as specvalue
                              FROM [dbo].[ProductSpecs] as ps
                              Inner Join Products as p
                              ON  p.ProductID = ps.ProductID
                              where ProductCode = @productCode
                                                        ";
                await using var cmd = new SqlCommand(sql, conn);

                cmd.Parameters.Add(new SqlParameter("@productCode", SqlDbType.VarChar) { Value = ProductCode });

                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {

                    listSpec.Add(new ProductSpec
                    {
                        SpecKey = reader.GetString(reader.GetOrdinal("speckey")),
                        SpecValue = reader.GetString(reader.GetOrdinal("specvalue"))
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("can't connection");
                Console.WriteLine(ex.Message);
            }

            return listSpec;
        }

        public static async Task<ProductMainInfo?> GetProductMainAsync(string connStr, string ProductCode)
        {
            ProductMainInfo? info = null;
            int ProductID = await ConnectDB.GetProductIDFromProductCode(connStr, ProductCode);
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @$"
                            SELECT
                                p.ProductName  AS Name,
                                p.Price        AS Price,
                                p.Stock        AS Stock,
                                b.BrandName    AS Brand,
                                (
                                    SELECT TOP (1) i.ImageUrl
                                    FROM dbo.ProductImages AS i
                                    WHERE i.ProductId = p.ProductId
                                    ORDER BY i.SortOder ASC, i.ImageId ASC
                                ) AS Thumbnail
                            FROM dbo.Products AS p
                            LEFT JOIN dbo.Brands AS b ON b.BrandId = p.BrandId
                            WHERE p.ProductID = @ProductID";

                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@ProductID", SqlDbType.Int) { Value = ProductID });
                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {

                    string name = reader.GetString(reader.GetOrdinal("Name"));
                    decimal price = reader.GetDecimal(reader.GetOrdinal("Price"));
                    int stock = reader.GetInt32(reader.GetOrdinal("Stock"));
                    string brand = reader.IsDBNull(reader.GetOrdinal("Brand")) ? string.Empty : reader.GetString(reader.GetOrdinal("Brand"));
                    string? thumb = reader.IsDBNull(reader.GetOrdinal("Thumbnail")) ? null : reader.GetString(reader.GetOrdinal("Thumbnail"));

                    info = new ProductMainInfo
                    {
                        Name = name,
                        Price = price,
                        Stock = stock,
                        Brand = brand,
                        Thumbnail = thumb
                    };

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("can't connection");
                Console.WriteLine(ex.Message);
            }

            return info;
        }
        public static async Task<List<string>> GetImageAsync(string connStr, string ProductCode)
        {
            var result = new List<string>();
            int ProductID = await ConnectDB.GetProductIDFromProductCode(connStr, ProductCode);
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();

                var sql = @"Select i.ImageUrl
                            From dbo.ProductImages AS i
                            where i.ProductID = @ProductID and i.SortOder > 1
                            Order BY i.SortOder, i.ImageID;
                            ";
                await using var cmd = new SqlCommand(sql, conn);

                cmd.Parameters.Add(new SqlParameter("@ProductID", SqlDbType.Int) { Value = ProductID });

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    if (!reader.IsDBNull(0))
                    {
                        var url = reader.GetString(0);
                        if (!string.IsNullOrWhiteSpace(url))
                            result.Add(url);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("can't connection");
                Console.WriteLine(ex.Message);
            }

            return result;
        }

        public static async Task<List<ProductGetList>> GetListProductForAdminPage(string connStr, ProductGetList productItem )
        {
            var list = new List<ProductGetList>();
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"SELECT [ProductCode] as productCode 
                                  ,[ProductName] as productName
                                  ,[Price] as price
                                  ,[Stock] as stock
                              FROM [dbo].[Products]
                            ";
                await using var cmd = new SqlCommand(sql, conn);
                var reader = await cmd.ExecuteReaderAsync();
                while(await reader.ReadAsync())
                {
                    list.Add(
                        new ProductGetList
                        {
                            productCode = reader.GetString(reader.GetOrdinal("productCode")),
                            productName = reader.GetString(reader.GetOrdinal("productName")),
                            price = reader.GetDecimal(reader.GetOrdinal("price")),
                            stock = reader.GetInt32(reader.GetOrdinal("productCode"))
                        });
                }
            }
            catch 
            {
                throw;
            }
            return list;
        }
        public static async Task<int?> createProduct(string connStr, CreateUpdateProduct productInfo, ProductSpec productSpec)
        {
            int? ProductID = null;
            try
            {
                var now = DateTime.UtcNow;
                var productCode = $"PRO-{now:yyyymmdd}-{Random.Shared.Next(1000, 9999)}";
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"
                            DECLARE
	                            @productID int		
                            BEGIN TRY
                                BEGIN TRAN;
                                -- Insert Product + lấy ProductID an toàn
	                            select @brandID = BrandID from Brands as b where b.BrandName = @brandName
	                            select @categoryID = CategoryID from Categories as ca where ca.CategoryName = @categoryName

                                INSERT INTO Products
                                (
                                    ProductCode,
                                    ProductName,
                                    Price,
                                    Descriptions,
                                    BrandID,
                                    CategoryID,
                                    Stock,
                                    Rating,
                                    CreateAt,
                                    UpdateAt
                                )
    
                                VALUES
                                (
                                    @productCode,
                                    @productName,
                                    @price,
                                    @description,
                                    @brandID,
                                    @categoryID,
                                    @stock,
                                    @rating,
                                    @CreateAt,
                                    @CreateAt
                                );
	                            SELECT @productID = ProductID from Products where ProductCode = @productCode
	                            INSERT INTO ProductSpecs
                                (
                                    SpecKey,
                                    SpecValue,
                                    ProductID
                                )
                                VALUES
                                (
                                    @SpecKey,
                                    @Specvalue,
                                    @productID
                                );

                                COMMIT TRAN;
                            END TRY
                            BEGIN CATCH
                                IF @@TRANCOUNT > 0 ROLLBACK TRAN;
                                THROW;
                            END CATCH;
                            SELECT ProductID as productID from Products where ProductCode = @productCode"";";

                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@productCode", SqlDbType.VarChar) { Value = productCode });
                cmd.Parameters.Add(new SqlParameter("@productName", SqlDbType.NVarChar) { Value = productInfo.Name });
                cmd.Parameters.Add(new SqlParameter("@price", SqlDbType.Decimal) { Value = productInfo.Price });
                cmd.Parameters.Add(new SqlParameter("@description", SqlDbType.NVarChar) { Value = productInfo.description });
                cmd.Parameters.Add(new SqlParameter("@stock", SqlDbType.Int) { Value = productInfo.Stock });
                cmd.Parameters.Add(new SqlParameter("@CreateAt", SqlDbType.NVarChar) { Value = now });
                cmd.Parameters.Add(new SqlParameter("@brandName", SqlDbType.NVarChar) { Value = productInfo.Brand });
                cmd.Parameters.Add(new SqlParameter("@categoryName", SqlDbType.NVarChar) { Value = productInfo.Category });
                cmd.Parameters.Add(new SqlParameter("@SpecKey", SqlDbType.NVarChar) { Value = productSpec.SpecKey });
                cmd.Parameters.Add(new SqlParameter("@Specvalue", SqlDbType.NVarChar) { Value = productSpec.SpecValue });
                var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    ProductID = reader.GetInt32(reader.GetOrdinal("productID"));
                }

                int imageQuantity = productInfo.image.Count;
                for (int i = 1; i <= imageQuantity; i++)
                {
                    string sql2 = @"BEGIN TRY
                                    BEGIN TRAN;
                                    DECLARE 
                                        @productID INT
	                                SELECT @productID = ProductID from Products where ProductCode = @productCode
                                    -- Product Image
                                    INSERT INTO ProductImages
                                    (
                                        ProductID,
                                        ImageURL,
                                        SortOder
                                    )
                                    VALUES
                                    (
                                        @productID,
                                        @ImageUrl,
                                        @sortOder
                                    );


                                    COMMIT TRAN;
                                END TRY
                                BEGIN CATCH
                                    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
                                    THROW;
                                END CATCH;";
                    await using var cmd2 = new SqlCommand(sql2, conn);
                    cmd2.Parameters.Add(new SqlParameter("@ImageUrl", SqlDbType.NVarChar) { Value = productInfo.image[i - 1] });
                    cmd2.Parameters.Add(new SqlParameter("@sortOder", SqlDbType.Int) { Value = i });
                    var affect = await cmd2.ExecuteNonQueryAsync();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return ProductID;
        }
        public static async Task<int?> updateProductDetail(string connStr, ProductItem productItem, ProductSpec productSpec)
        {
            int ProductID = await ConnectDB.GetProductIDFromProductCode(connStr, productItem.id);
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"";
                await using var cmd = new SqlCommand(sql, conn);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return ProductID;
        }
        public static async Task<int?> deleteProductDetail(string connStr, string productCode)
        {
            int ProductID = await ConnectDB.GetProductIDFromProductCode(connStr, productCode);
            try
            {
                using var conn = ConnectDB.Create(connStr);
                await conn.OpenAsync();
                var sql = @"
                            BEGIN TRY
                                BEGIN TRAN;

                                DELETE FROM dbo.ProductImages
                                WHERE ProductID = @productID;

                                DELETE FROM dbo.ProductSpecs
                                WHERE ProductID = @productID;

                                DELETE FROM dbo.Products
                                WHERE ProductID = @productID;

                                COMMIT TRAN;
                            END TRY
                            BEGIN CATCH
                                IF @@TRANCOUNT > 0
                                    ROLLBACK TRAN;

                                THROW; -- trả lỗi ra ngoài
                            END CATCH;";
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add(new SqlParameter("@productID", SqlDbType.Int) { Value = ProductID });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return ProductID;
        }
        
        //return object
        public static async Task<ProductDetail?> ReadAsDtoAsync(string connStr, string ProductCode)
        {
            var main = await GetProductMainAsync(connStr, ProductCode);
            if (main is null) return null;

            var specs = await GetProductSpecAsync(connStr, ProductCode);
            var images = await GetImageAsync(connStr, ProductCode);

            return new ProductDetail
            {
                ProductId = ProductCode,
                Name = main.Name,
                Price = main.Price,
                Stock = main.Stock,
                Brand = main.Brand,
                Thumbnail = main.Thumbnail,
                Specs = specs,
                Images = images
            };
        }
        // return string Json
        public static async Task<string> ReadAsJsonAync(string connStr, string ProductCode)
        {
            int productID = await ConnectDB.GetProductIDFromProductCode(connStr, ProductCode);
            var main = await GetProductMainAsync(connStr, ProductCode);
            if (main is null)
            {
                return JsonSerializer.Serialize(new { productID, message = "product not found" });
            }
            var specs = await GetProductSpecAsync(connStr, ProductCode);
            var images = await GetImageAsync(connStr, ProductCode);
            var dto = new ProductDetail
            {
                ProductId = ProductCode,
                Name = main.Name,
                Price = main.Price,
                Stock = main.Stock,
                Brand = main.Brand,
                Thumbnail = main.Thumbnail,
                Specs = specs,
                Images = images
            };

            var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            return json;
        }

    }
}


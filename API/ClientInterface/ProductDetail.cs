using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using WebsiteComputer.Database;
namespace API.ClientInterface
{
    [ApiController]
    [Route("api/productDetail")]

    public class ProductDetail : ControllerBase
    {
        private readonly IConfiguration _config;
        public ProductDetail(IConfiguration config)
        {
            _config = config;
        }
        private string connStr =>
            _config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");

        [HttpGet("{ProductCode}")]
        public async Task<IActionResult> GetProduct(string ProductCode)
        {
            var product = await DBProductDetail.ReadAsDtoAsync(connStr, ProductCode);

            return Ok(product);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using  WebsiteComputer.Models;
using static WebsiteComputer.Database.DBClient;
namespace API.ClientInterface
{
    [ApiController]
    [Route("api/login")]
    public class SignUp : ControllerBase
    {
        private readonly IConfiguration _config;

        public SignUp(IConfiguration config)
        {
            _config = config;
        }

        private string connStr =>
            _config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
        [HttpPost]
        public async Task<IActionResult> CreateNewClient([FromBody] ClientDtos.ClientDetail client)
        {

            try
            {
                var clientID = await CreateClient(connStr, client);
                return Ok(clientID);
            }
            catch(Exception e)
            {
                return BadRequest(e);
            }

        }
    }
}

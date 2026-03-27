using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using WebsiteComputer.Database;
using WebsiteComputer.Models;

namespace API.ClientInterface
{
    [ApiController]
    [Route("api/login")]
    public class Login : ControllerBase
    {
        private readonly IConfiguration _config;

        public Login(IConfiguration config)
        {
            _config = config;
        }

        private string connStr =>
            _config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
        [HttpGet]
        public async Task<IActionResult> LoginAction([FromBody] ClientDtos.ClientLogin client)
        {

            try
            {
                var clientID = await DBClient.Login (connStr, client);
                return Ok(clientID);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }

        }
    }
}

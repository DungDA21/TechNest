using Database.DBAdmin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using System.Text.Encodings.Web;
using System.Text.Json;
using WebsiteComputer.Database.DBAdmin;
using WebsiteComputer.Models.Policy;

namespace API.Admin
{
    [ApiController]
    [Route("api/Admin/Discount")]
    public class GuaranteeAdmin : ControllerBase
    {
        private readonly IConfiguration _config;

        public GuaranteeAdmin(IConfiguration config)
        {
            _config = config;
        }

        private string connStr =>
            _config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGuarantee(string id)
        {
            var results = DBAdminGuarantee.ReadGuarantee(connStr, id);
            return Ok(results);
        }
        [HttpGet]
        public async Task<IActionResult> getGuaranteeList()
        {
            var results = DBAdminGuarantee.ReadListGuarantee(connStr);
            return Ok(results);            
        }
        [HttpPost]
        public async Task<IActionResult> postGuarantee([FromBody]Guarantee.GuaranteeProduct guarantee)
        {
            var results = DBAdminGuarantee.CreateGuarantee(connStr, guarantee);
            return Ok(results);  
        }
        [HttpPut]
        public async Task<IActionResult> putGuarantee([FromBody]Guarantee.GuaranteeProduct guarantee)
        {
            var results = DBAdminGuarantee.UpdateGuarantee(connStr, guarantee);
            return Ok(results);             
        }
        [HttpDelete("{Id}")]
        public async Task<IActionResult> deleteGuarantee(string id)
        {
            var results = DBAdminGuarantee.DeleteGuarantee(connStr, id);
            return Ok(results);
        }

    }
}

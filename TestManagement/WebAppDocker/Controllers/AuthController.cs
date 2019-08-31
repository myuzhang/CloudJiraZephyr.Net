using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using ZephyrCloudHelper;
using ZephyrCloudHelper.Models;

namespace WebAppDocker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IJiraCloudApi _jiraCloudApi;
        private readonly IZephyrCloudApi _zephyrCloudApi;

        public AuthController(IJiraCloudApi jiraCloudApi, IZephyrCloudApi zephyrCloudApi)
        {
            _jiraCloudApi = jiraCloudApi;
            _zephyrCloudApi = zephyrCloudApi;
        }

        // POST api/auth/jira
        [HttpPost("jira")]
        public void GetJiraAuth([FromBody] JiraApi auth)
        {
        }

        // POST api/auth
        [HttpPost("zephyr")]
        public void GetZephyrAuth([FromBody] Zapi auth)
        {
        }

        // GET api/auth
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/auth/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/auth
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/auth/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/auth/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

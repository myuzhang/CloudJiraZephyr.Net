using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ZephyrCloudHelper.Models;

namespace WebAppDocker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ITestManagerService _testManagerService;
        
        public AuthController(ITestManagerService testManagerService)
        {
            _testManagerService = testManagerService;
        }

        // POST api/auth/jira
        [HttpPost("jira")]
        public async Task<IActionResult> SetJiraAuth([FromBody] JiraApi auth)
        {
            if (string.IsNullOrWhiteSpace(auth.Authentication) ||
                string.IsNullOrWhiteSpace(auth.JiraCloudUrl) ||
                string.IsNullOrWhiteSpace(auth.ProjectKey))
                return BadRequest("Please provide correct full info of auth for Jira");

            _testManagerService.SetJiraCloudApi(auth);
            return Ok("Jira auth has been setup");
        }

        // POST api/auth
        [HttpPost("zephyr")]
        public async Task<IActionResult> SetZephyrAuth([FromBody] Zapi auth)
        {
            if (string.IsNullOrWhiteSpace(auth.AccessKey) ||
                string.IsNullOrWhiteSpace(auth.SecretKey) ||
                string.IsNullOrWhiteSpace(auth.User))
                return BadRequest("Please provide correct full info of auth for Zephyr");

            _testManagerService.SetZephyrCloudApi(auth);
            return Ok("Zephyr auth has been setup");
        }
    }
}

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestManager;

namespace WebAppDocker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExecutionController : ControllerBase
    {
        private readonly ITestManagerService _testManagerService;

        public ExecutionController(ITestManagerService testManagerService)
        {
            _testManagerService = testManagerService;
        }

        // POST: api/Cycle
        [HttpPost("result")]
        public async Task<IActionResult> UpdateTestResult([FromBody] TestInfo testInfo)
        {
            if (string.IsNullOrWhiteSpace(testInfo.Title) &&
                string.IsNullOrWhiteSpace(testInfo.TestKey))
                return BadRequest("Please specify either JIRA issue Title or TestKey");

            var (success, message) = await _testManagerService.ExecutionManager.UpdateTestResult(testInfo);
            if (success)
                return Ok(
                    $"The test {message} has been updated to {testInfo.Result} in {testInfo.TestVersion}/{testInfo.TestCycle}");

            return BadRequest($"Failed to update test - {message}");
        }
    }
}
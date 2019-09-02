using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebAppDocker.Models;

namespace WebAppDocker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CycleController : ControllerBase
    {
        private readonly ITestManagerService _testManagerService;

        public CycleController(ITestManagerService testManagerService)
        {
            _testManagerService = testManagerService;
        }

        // POST: api/Cycle
        [HttpPost]
        public async Task<IActionResult> CreateTestCycle([FromBody] TestCycleInfo testCycleInfo)
        {
            if (string.IsNullOrWhiteSpace(testCycleInfo.ProjectKey) ||
                string.IsNullOrWhiteSpace(testCycleInfo.TestCycle) ||
                string.IsNullOrWhiteSpace(testCycleInfo.TestVersion))
                return BadRequest("Please provide ProjectKey, TestCycle and TestVersion");

            _testManagerService.TestSuiteManager.CreateTestCycle(testCycleInfo.ProjectKey, testCycleInfo.TestVersion, testCycleInfo.TestCycle);
            return Ok($"The Test cycle {testCycleInfo.TestCycle} was build under {testCycleInfo.ProjectKey}/{testCycleInfo.TestVersion}"); 
        }
    }
}

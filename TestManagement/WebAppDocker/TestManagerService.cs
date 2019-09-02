using TestManager;
using ZephyrCloudHelper;
using ZephyrCloudHelper.Models;

namespace WebAppDocker
{
    public class TestManagerService: ITestManagerService
    {
        private IJiraCloudApi _jiraCloudApi { get; set; }

        private IZephyrCloudApi _zephyrCloudApi { get; set; }

        public ITestSuiteManager TestSuiteManager =>
            new TestSuiteManager(_jiraCloudApi, _zephyrCloudApi);

        public IExecutionManager ExecutionManager =>
            new ExecutionManager(_jiraCloudApi, _zephyrCloudApi, TestSuiteManager);

        public void SetJiraCloudApi(JiraApi jiraApi) => _jiraCloudApi = new JiraCloudApi(jiraApi);

        public void SetZephyrCloudApi(Zapi zapi) => _zephyrCloudApi = new ZephyrCloudApi(zapi);

        public void CreateTestCycle(string projectKey, string testVersion, string testCycle) =>
            TestSuiteManager.CreateTestCycle(projectKey, testVersion, testCycle);

        public void UpdateTestResult(TestInfo testInfo) =>
            ExecutionManager.UpdateTestResult(testInfo);
    }
}

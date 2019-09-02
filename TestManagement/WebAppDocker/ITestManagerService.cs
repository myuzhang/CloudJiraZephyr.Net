using TestManager;
using ZephyrCloudHelper.Models;

namespace WebAppDocker
{
    public interface ITestManagerService
    {
        ITestSuiteManager TestSuiteManager { get; }

        IExecutionManager ExecutionManager { get; }

        void SetJiraCloudApi(JiraApi jiraApi);

        void SetZephyrCloudApi(Zapi zapi);
    }
}
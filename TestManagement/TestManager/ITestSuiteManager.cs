using System.Threading.Tasks;

namespace TestManager
{
    public interface ITestSuiteManager
    {
        Task<TestSuiteInfo> GetTestSuiteInfo(string projectKey, string testVersion, string testCycle);
    }
}

using System.Threading.Tasks;

namespace SqsLambda
{
    public interface ITestSuiteManager
    {
        Task<TestSuiteInfo> GetTestSuiteInfo(string projectKey, string testVersion, string testCycle);
    }
}

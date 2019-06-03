using System.Threading.Tasks;

namespace SqsLambda
{
    public interface IExecutionManager
    {
        Task<(bool, string)> UpdateTestResult(TestInfo testInfo);
    }
}
using System.Threading.Tasks;

namespace TestManager
{
    public interface IExecutionManager
    {
        Task<(bool, string)> UpdateTestResult(TestInfo testInfo);
    }
}
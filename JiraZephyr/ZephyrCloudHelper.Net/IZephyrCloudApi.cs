using System.Collections.Generic;
using ZephyrCloudHelper.Net.Models;
using ZephyrCloudHelper.Net.Models.Jira;
using ZephyrCloudHelper.Net.Models.Zephyr;

namespace ZephyrCloudHelper.Net
{
    public interface IZephyrCloudApi
    {
        void UpdateExecution(Execution execution, JiraTestOutcome status);
        void CloneTestCycle(string cycleName, long projectId, long fromVersionId, long toVersionId);
        void CreateTestCycle(string cycleName, long projectId, long versionId);
        Cycle GetTestCycle(string cycleName, long versionId, long projectId);
        bool AddTestToCycle(Issue issue, Cycle cycle);
        IList<Execution> GetExecutionsFromCycle(long issueId, string cycleId, long projectId);
        IList<Execution> GetExecutionsForIssue(long issueId, long projectId);
        void AddStepsToTest(long issueId, long projectId, IList<Step> steps, bool overwritten = false);
        void DeleteTestSteps(long issueId, long projectId);
        IList<Execution> GetExecutionsForIssue(long issueId, long projectId, int offset, out int pageSize, out int totalCount);
    }
}
using System.Collections.Generic;
using ZephyrCloudHelper.Models;
using ZephyrCloudHelper.Models.Jira;
using ZephyrCloudHelper.Models.Zephyr;

namespace ZephyrCloudHelper
{
    public interface IAtlassianService
    {
        long ProjectId { get; }
        void UpdateTestStatus(string testCaseKey, string versionName, JiraTestOutcome outcome);
        void UpdateTestCaseInVersion(string testCaseKey, string versionName, JiraTestOutcome status);
        Issue CreateTestCase(IssueCreation issueCreation);
        Issue UpdateTestCase(string issueKey, IssueUpdate issueUpdate);
        Version CreateVersion(string versionName, string projectKey);
        Version GetReleaseVersion(string versionName);
        Cycle GetTestCycle(long versionId, long projectId, string cycleName);
        void AddTestCaseToCycle(string issueKey, string cycleName, string versionName, long projectId);
        void AddTestCaseToCycle(Issue issue, Cycle cycle);
        void AddStepsToTestCase(string issuekey, IList<Step> steps, bool overwritten = false);
        void CloneTestCycle(string cycleName, string fromVersionName, string toVersion);
        Issue CreateTestCase(string projectKey, IssueInCycle issueInCycle, string reporter = null);
    }
}
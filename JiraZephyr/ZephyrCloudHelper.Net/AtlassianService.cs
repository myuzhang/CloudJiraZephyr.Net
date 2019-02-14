using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ZephyrCloudHelper.Net.Models;
using ZephyrCloudHelper.Net.Models.Jira;
using ZephyrCloudHelper.Net.Models.Zephyr;
using Version = ZephyrCloudHelper.Net.Models.Jira.Version;

namespace ZephyrCloudHelper.Net
{
    /// <summary>
    /// The service for Atlassian - Jira and Zephyr
    /// </summary>
    public class AtlassianService
    {
        private readonly ZephyrCloudApi _zephyrCloudApi;

        private readonly JiraCloudApi _jiraCloudApi;

        public AtlassianService(Zapi zapi, JiraApi jiraApi)
        {
            _zephyrCloudApi = new ZephyrCloudApi(zapi);
            _jiraCloudApi = new JiraCloudApi(jiraApi);
        }

        public long ProjectId => _jiraCloudApi.Project.Id;

        public void UpdateTestCaseInVersion(string testCaseKey, string versionName, JiraTestOutcome status)
        {
            var issue = _jiraCloudApi.GetIssue(testCaseKey);
            if (issue == null)
            {
                Debug.WriteLine($"Test case {testCaseKey} is not found in Jira");
                return;
            }

            var version = _jiraCloudApi.GetVersion(versionName);
            if (version == null)
            {

                Debug.WriteLine($"Test version {versionName} is not found in Jira");
                return;
            }

            var executions = _zephyrCloudApi.GetExecutionsForIssue(issue.Id, issue.ProjectId);
            var executionsInVersion = executions.Where(e => e.VersionId.Equals(version.Id));

            foreach (var execution in executionsInVersion)
            {
                _zephyrCloudApi.UpdateExecution(execution, status);
            }
        }

        public Issue CreateTestCase(IssueCreation issueCreation)
        {
            var issues = _jiraCloudApi.QueryIssueKeys(issueCreation.fields.summary, true);
            if (issues != null && issues.Count > 0)
            {
                var issueKeys = string.Join(", ", issues.ToArray());
                Debug.WriteLine($"Test case with summary \"{issueCreation.fields.summary}\" already exists as {issueKeys} in Jira");
                return _jiraCloudApi.GetIssue(issues.First());
            }

            var createdIssue = _jiraCloudApi.CreateIssue(issueCreation);
            Debug.WriteLine($"Test case with summary \"{issueCreation.fields.summary}\" was created as {createdIssue.Key} in Jira");
            return createdIssue;
        }

        public Issue UpdateTestCase(string issueKey, IssueUpdate issueUpdate)
        {
            var issue = _jiraCloudApi.GetIssue(issueKey);
            if (issue == null) return null;
            _jiraCloudApi.UpdateIssue(issueKey, issueUpdate);
            Debug.WriteLine($"Test case {issueKey} was updated in Jira");
            return issue;
        }

        public Version CreateVersion(string versionName, string projectKey)
        {
            try
            {
                var version = _jiraCloudApi.GetVersion(versionName);
                if (version == null)
                {
                    var versionCreation = new VersionCreation
                    {
                        description = "Created from Automation Framework",
                        archived = false,
                        name = versionName,
                        project = projectKey,
                        projectId = ProjectId,
                        startDate = DateTime.Today.ToString("yyyy-MM-dd")
                    };
                    version = _jiraCloudApi.CreateVersion(versionCreation);
                }

                return version;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Create version {versionName} failed at: {e.Message}");
                return null;
            }
        }

        public Version GetReleaseVersion(string versionName) =>
            _jiraCloudApi.GetVersion(versionName);

        public Cycle GetTestCycle(long versionId, long projectId, string cycleName) =>
            _zephyrCloudApi.GetTestCycle(cycleName, versionId, projectId);

        public void AddTestCaseToCycle(string issueKey, string cycleName, string versionName, long projectId)
        {
            var issue = _jiraCloudApi.GetIssue(issueKey);
            if (issue == null)
            {
                Debug.WriteLine($"Test case {issueKey} is not found in Jira");
                return;
            }

            var version = _jiraCloudApi.GetVersion(versionName);
            if (version == null)
            {
                Debug.WriteLine($"Test version \"{versionName}\" is not found in Jira");
                return;
            }

            var cycle = _zephyrCloudApi.GetTestCycle(cycleName, version.Id, projectId);
            if (cycle == null)
            {
                Debug.WriteLine($"Test cycle \"{cycleName}\" is not found in Jira");
                return;
            }

            var executions = _zephyrCloudApi.GetExecutionsFromCycle(issue.Id, cycle.Id, projectId);
            if (executions != null && executions.Count > 0)
            {
                Debug.WriteLine(
                    $"Test case {issueKey} already exists in cycle \"{cycleName}\" under version \"{versionName}\" in Jira");
                return;
            }

            var status = _zephyrCloudApi.AddTestToCycle(issue, cycle);
            if (status)
                Debug.WriteLine(
                    $"Test case {issueKey} is added to cycle \"{cycleName}\" under version \"{versionName}\" in Jira");
            else
                Debug.WriteLine(
                    $"Failed in adding test case {issueKey} to cycle \"{cycleName}\" under version \"{versionName}\" in Jira");
        }

        public void AddTestCaseToCycle(Issue issue, Cycle cycle)
        {
            var version = _jiraCloudApi.GetVersion(cycle.VersionId);
            var executions = _zephyrCloudApi.GetExecutionsFromCycle(issue.Id, cycle.Id, cycle.ProjectId);
            if (executions != null && executions.Count > 0)
            {
                Debug.WriteLine(
                    $"Test case {issue.Key} already exists in cycle \"{cycle.Name}\" under version \"{version.Name}\" in Jira");
                return;
            }

            var status = _zephyrCloudApi.AddTestToCycle(issue, cycle);
            if (status)
                Debug.WriteLine(
                    $"Test case {issue.Key} is added to cycle \"{cycle.Name}\" under version \"{version.Name}\" in Jira");
            else
                Debug.WriteLine(
                    $"Failed in adding test case {issue.Key} to cycle \"{cycle.Name}\" under version \"{version.Name}\" in Jira");
        }

        public void AddStepsToTestCase(string issuekey, IList<Step> steps, bool overwritten = false)
        {
            var issue = _jiraCloudApi.GetIssue(issuekey);
            if (issue == null)
            {
                Debug.WriteLine($"No test case {issuekey} found in Jira");
                return;
            }
            _zephyrCloudApi.AddStepsToTest(issue.Id, ProjectId, steps, overwritten);
        }

        public void CloneTestCycle(string cycleName, string fromVersionName, string toVersion)
        {
            var from = _jiraCloudApi.GetVersion(fromVersionName);
            var to = _jiraCloudApi.GetVersion(toVersion);
            if (from != null && to != null)
            {
                _zephyrCloudApi.CloneTestCycle(cycleName, ProjectId, from.Id, to.Id);
            }
        }
    }
}

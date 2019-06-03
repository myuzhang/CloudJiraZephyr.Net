using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ZephyrCloudHelper.Models;
using ZephyrCloudHelper.Models.Jira;
using ZephyrCloudHelper.Models.Zephyr;
using Version = ZephyrCloudHelper.Models.Jira.Version;

namespace ZephyrCloudHelper
{
    /// <summary>
    /// The service for Atlassian - Jira and Zephyr
    /// </summary>
    public class AtlassianService : IAtlassianService
    {
        private readonly ZephyrCloudApi _zephyrCloudApi;

        private readonly JiraCloudApi _jiraCloudApi;

        public AtlassianService(Zapi zapi, JiraApi jiraApi)
        {
            _zephyrCloudApi = new ZephyrCloudApi(zapi);
            _jiraCloudApi = new JiraCloudApi(jiraApi);
        }

        public long ProjectId => _jiraCloudApi.Project.Id;

        public void UpdateTestStatus(string testCaseKey, string versionName, JiraTestOutcome outcome)
        {
            try
            {
                UpdateTestCaseInVersion(testCaseKey, versionName, outcome);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Update test({testCaseKey}) status({outcome.ToString()}) to Zephyr failed due to {e.Message}");
            }
        }

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

        public Issue CreateTestCase(string projectKey, IssueInCycle issueInCycle, string reporter = null)
        {
            Issue issue = null;
            try
            {
                var description = $"Please refer to automation test - {issueInCycle.TestName}()";

                // step 1, create a test case in Jira
                if (string.IsNullOrWhiteSpace(issueInCycle.Key))
                {
                    var issueCreation = new IssueCreation
                    {
                        fields = new Fields
                        {
                            project = new ProjectField { key = projectKey },
                            summary = issueInCycle.TestSummary,
                            description = description,
                            issuetype = new Issuetype { name = "Test" },
                            reporter = new Reporter { name = reporter },
                            labels = issueInCycle.Labels
                        }
                    };
                    issueInCycle.Overwritten = false;
                    issue = CreateTestCase(issueCreation);
                }
                else // step 1, update a test case in Jira
                {
                    var issueUpdate = new IssueUpdate
                    {
                        fields = new UpdateFields
                        {
                            description = description,
                            labels = issueInCycle.Labels,
                            summary = issueInCycle.TestSummary
                        }
                    };
                    issueInCycle.Overwritten = true;
                    issue = UpdateTestCase(issueInCycle.Key, issueUpdate);
                }

                if (issue != null)
                {
                    // step 2, add test steps to the issue in Zephyr
                    if (issueInCycle.Steps != null && issueInCycle.Steps.Count > 0)
                        AddStepsToTestCase(issue.Key, issueInCycle.Steps, issueInCycle.Overwritten);

                    // step 3, add the test case to the constrain of version-cycles
                    if (issueInCycle.CycleNames != null && issueInCycle.CycleNames.Count > 0)
                    {
                        if (!string.IsNullOrWhiteSpace(issueInCycle.VersionName))
                        {
                            var version = GetReleaseVersion(issueInCycle.VersionName);
                            if (version != null)
                            {
                                foreach (var cycleName in issueInCycle.CycleNames)
                                {
                                    var cycle = GetTestCycle(version.Id, version.ProjectId, cycleName);
                                    if (cycle != null) AddTestCaseToCycle(issue, cycle);
                                }
                            }
                        }
                    }
                }

                return issue;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Create test {issueInCycle.TestName} in Jira failed at: {e.Message}");
                return issue;
            }
        }
    }
}

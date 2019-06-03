using System;
using System.Diagnostics;
using ZephyrCloudHelper.Net.Models;
using ZephyrCloudHelper.Net.Models.Jira;

namespace ZephyrCloudHelper.Net
{
    /// <summary>
    /// The Jira service
    /// </summary>
    public class TestManagementHelper
    {
        private readonly AtlassianService _service;
        
        public TestManagementHelper(Zapi zapi, JiraApi jiraApi)
        {
            _service = new AtlassianService(zapi, jiraApi);
        }

        public void UpdateTestStatus(string testCaseKey, string versionName, JiraTestOutcome outcome)
        {
            try
            {
                _service.UpdateTestCaseInVersion(testCaseKey, versionName, outcome);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Update test({testCaseKey}) status({outcome.ToString()}) to Zephyr failed due to {e.Message}");
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
                    issue = _service.CreateTestCase(issueCreation);
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
                    issue = _service.UpdateTestCase(issueInCycle.Key, issueUpdate);
                }

                if (issue != null)
                {
                    // step 2, add test steps to the issue in Zephyr
                    if (issueInCycle.Steps != null && issueInCycle.Steps.Count > 0)
                        _service.AddStepsToTestCase(issue.Key, issueInCycle.Steps, issueInCycle.Overwritten);

                    // step 3, add the test case to the constrain of version-cycles
                    if (issueInCycle.CycleNames != null && issueInCycle.CycleNames.Count > 0)
                    {
                        if (!string.IsNullOrWhiteSpace(issueInCycle.VersionName))
                        {
                            var version = _service.GetReleaseVersion(issueInCycle.VersionName);
                            if (version != null)
                            {
                                foreach (var cycleName in issueInCycle.CycleNames)
                                {
                                    var cycle = _service.GetTestCycle(version.Id, version.ProjectId, cycleName);
                                    if (cycle != null) _service.AddTestCaseToCycle(issue, cycle);
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

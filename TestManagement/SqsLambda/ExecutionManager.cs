using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZephyrCloudHelper;
using ZephyrCloudHelper.Models;
using ZephyrCloudHelper.Models.Jira;
using ZephyrCloudHelper.Models.Zephyr;

namespace SqsLambda
{
    public class ExecutionManager : IExecutionManager
    {
        private readonly IJiraCloudApi _jiraCloudApi;
        private readonly IZephyrCloudApi _zephyrCloudApi;
        private readonly ITestSuiteManager _testSuiteManager;

        public ExecutionManager(IJiraCloudApi jiraCloudApi, IZephyrCloudApi zephyrCloudApi, ITestSuiteManager testSuiteManager)
        {
            _jiraCloudApi = jiraCloudApi;
            _zephyrCloudApi = zephyrCloudApi;
            _testSuiteManager = testSuiteManager;
        }

        public async Task<(bool,string)> UpdateTestResult(TestInfo testInfo)
        {
            string jiraTicket = String.Empty;
            try
            {
                var testSuitInfo = await _testSuiteManager.GetTestSuiteInfo(
                    testInfo.ProjectKey, testInfo.TestVersion, testInfo.TestCycle);
                var project = testSuitInfo.Project;
                var cycle = testSuitInfo.Cycle;

                IList<Execution> executions = null;

                var issueKeys = _jiraCloudApi.QueryIssueKeys(testInfo.Title, true);

                // if no issues found, the issue will be created
                if (issueKeys == null || issueKeys.Count == 0)
                {
                    var issue = _jiraCloudApi.CreateIssue(new IssueCreation
                    {
                        fields = new Fields
                        {
                            project = new ProjectField
                            {
                                key = project.Key
                            },
                            summary = testInfo.Title,
                            description = testInfo.Description,
                            issuetype = new Issuetype
                            {
                                name = "Test"
                            },
                            reporter = null
                        }
                    });

                    if (issue == null)
                        throw new AggregateException($"The test case for '{testInfo.Title}' is not created successfully.");

                    // assign the issue to test cycle to create an execution
                    executions = await AddTestToCycle(issue, cycle, project);
                    jiraTicket = issue.Key;
                }
                else
                {
                    jiraTicket = issueKeys.FirstOrDefault();
                    foreach (var issueKey in issueKeys)
                    {
                        var issue = _jiraCloudApi.GetIssue(issueKey);
                        executions = _zephyrCloudApi.GetExecutionsFromCycle(issue.Id, cycle.Id, project.Id);

                        // if no execution found, assign the issue to test cycle to create an execution
                        if (executions == null || executions.Count == 0)
                        {
                            executions = await AddTestToCycle(issue, cycle, project);
                        }
                    }
                }

                if (executions != null)
                {
                    foreach (var execution in executions)
                    {
                        _zephyrCloudApi.UpdateExecution(execution, ToJiraTestOutcome(testInfo.Result));
                    }
                }

                return (true, jiraTicket);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Update test result failed at {e}");
            }

            return (false, jiraTicket);
        }

        private JiraTestOutcome ToJiraTestOutcome(string outcome) =>
            Enum.TryParse(outcome, true, out JiraTestOutcome jiraOutcome) ? jiraOutcome : JiraTestOutcome.InProgress;

        // wait 10 seconds for Zephyr to add test into the test cycle
        private async Task<IList<Execution>> AddTestToCycle(Issue issue, Cycle cycle, Project project)
        {
            var retry = 5;
            _zephyrCloudApi.AddTestToCycle(issue, cycle);

            do
            {
                var executions = _zephyrCloudApi.GetExecutionsFromCycle(issue.Id, cycle.Id, project.Id);
                if (executions != null && executions.Count != 0 || --retry < 0)
                {
                    return executions;
                }
                await Task.Delay(TimeSpan.FromSeconds(2));
            } while (true);
        }

        //private IList<Execution> AddTestToCycle(Issue issue, Cycle cycle, Project project)
        //{
        //    IList<Execution> executions = null;
        //    _zephyrCloudApi.AddTestToCycle(issue, cycle);
        //    SpinWait.SpinUntil(() =>
        //    {
        //        executions = _zephyrCloudApi.GetExecutionsFromCycle(issue.Id, cycle.Id, project.Id);
        //        return executions != null && executions.Count != 0;
        //    }, TimeSpan.FromSeconds(5));

        //    return executions;
        //}

        //private string JoinLines(IList<string> lines, bool isBoldGherkinKeyword = false)
        //{
        //    if (lines != null && lines.Count > 0)
        //    {
        //        if (isBoldGherkinKeyword)
        //        {
        //            for (var i = 0; i < lines.Count; i++)
        //            {
        //                lines[i] = BoldGherkinKeyword(lines[i]);
        //            }
        //        }
        //        return string.Join("\r\n", lines);
        //    }

        //    return null;
        //}

        //private string BoldGherkinKeyword(string line)
        //{
        //    string[] keywords = new[] { "Given", "When", "Then", "But", "And", "Examples", "Scenario", "Feature", "Background", "Scenario Outline" };
        //    foreach (var keyword in keywords)
        //    {
        //        if (line.TrimStart().StartsWith(keyword))
        //        {
        //            var place = line.IndexOf(keyword, StringComparison.CurrentCulture);
        //            return line.Remove(place, keyword.Length).Insert(place, $"*{keyword}*");
        //        }
        //    }

        //    return line;
        //}
    }
}

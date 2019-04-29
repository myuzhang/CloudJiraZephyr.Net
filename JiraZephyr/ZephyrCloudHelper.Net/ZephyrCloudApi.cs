using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using ZephyrCloudHelper.Net.Models;
using ZephyrCloudHelper.Net.Models.Jira;
using ZephyrCloudHelper.Net.Models.Zephyr;

namespace ZephyrCloudHelper.Net
{
    /// <summary>
    /// Jira API related methods
    /// Zephyr Cloud REST API - https://zfjcloud.docs.apiary.io/#
    /// </summary>
    public class ZephyrCloudApi
    {
        public const string Execution = "execution";

        private const string Id = "id";
        private const string Name = "name";
        private const string ProjectId = "projectId";
        private const string VersionId = "versionId";
        private const string ZapiSource = "/public/rest/api/1.0";
        private const string ZapiCloudUrl = "https://prod-api.zephyr4jiracloud.com/connect";

        private readonly ZapiClient _zapi;

        public ZephyrCloudApi(Zapi zapi)
        {
            _zapi = new ZapiClient(zapi.User, zapi.AccessKey, zapi.SecretKey, ZapiCloudUrl);
        }

        public void UpdateExecution(Execution execution, JiraTestOutcome status)
        {
            var updateExecution = new UpdateExecution
            {
                status = new Status { id = (int)status },
                id = execution.Id,
                projectId = execution.ProjectId,
                issueId = execution.IssueId,
                cycleId = execution.CycleId,
                versionId = execution.VersionId
            };

            UpdateExecution(execution.Id, updateExecution);
        }

        public void CloneTestCycle(string cycleName, long projectId, long fromVersionId, long toVersionId)
        {
            var newCycle = GetTestCycle(cycleName, toVersionId, projectId);
            if (newCycle != null) return; // don't clone to version if cycle exists

            var originalCycle = GetTestCycle(cycleName, fromVersionId, projectId);
            var cycleCreation = new CycleCreation
            {
                name = cycleName,
                description = "The cycle is created automatically",
                environment = "Test",
                projectId = projectId,
                versionId = toVersionId
            };
            if (originalCycle != null)
            {
                CloneCycle(cycleCreation, originalCycle.Id);
            }
        }

        public void CreateTestCycle(string cycleName, long projectId, long versionId)
        {
            var newCycle = new CycleCreation
            {
                name = cycleName,
                description = "The cycle is created automatically",
                environment = "Test",
                projectId = projectId,
                versionId = versionId
            };

            CreateCycle(newCycle);
        }

        public Cycle GetTestCycle(string cycleName, long versionId, long projectId)
        {
            var cycles = GetCycles(versionId, projectId);
            var cycleList = (from c in cycles as JArray
                select new Cycle
                {
                    Id = (string)c[Id],
                    Name = (string)c[Name],
                    ProjectId = (long)c[ProjectId],
                    VersionId = (long)c[VersionId]
                }).ToList();

            return cycleList.FirstOrDefault(c =>
                c.Name.Equals(cycleName, StringComparison.CurrentCultureIgnoreCase));
        }

        public bool AddTestToCycle(Issue issue, Cycle cycle)
        {
            var response = AddTestToCycle(cycle.Id, new TestToCycle
            {
                projectId = cycle.ProjectId,
                versionId = cycle.VersionId,
                issues = new List<long> { issue.Id },
                method = "1"
            });

            return response != null;
        }

        public IList<Execution> GetExecutionsFromCycle(long issueId, string cycleId, long projectId)
        {
            var executions = GetExecutionsForIssue(issueId, projectId);
            if (executions != null && executions.Count > 0)
                return executions.Where(e => e.CycleId.Equals(cycleId)).ToList();

            return null;
        }

        public IList<Execution> GetExecutionsForIssue(long issueId, long projectId)
        {
            var offset = 0;
            var allExecutions = new List<Execution>();

            var executions = GetExecutionsForIssue(issueId, projectId, offset, out var pageSize, out var totalCount);
            if (executions != null)
            {
                allExecutions.AddRange(executions);
                for (offset = pageSize; offset < totalCount; offset += pageSize)
                {
                    executions = GetExecutionsForIssue(issueId, projectId, offset, out pageSize, out totalCount);
                    if (executions != null)
                        allExecutions.AddRange(executions);
                }
            }

            return allExecutions;
        }

        public void AddStepsToTest(long issueId, long projectId, IList<Step> steps, bool overwritten = false)
        {
            if (overwritten)
                DeleteTestSteps(issueId, projectId);

            foreach (var testStep in steps)
            {
                var response = CreateNewStep(issueId, projectId, testStep);
                if (response == null) Console.WriteLine($"Create test step \"{testStep.step}\" failed");
            }
        }

        public void DeleteTestSteps(long issueId, long projectId)
        {
            var response = GetAllTestSteps(issueId, projectId);
            if (response == null) return;

            var steps = (from s in response as JArray
                select new TestStep
                {
                    id = (string) s[Id],
                    data = (string) s["data"],
                    result = (string) s["result"],
                    step = (string) s["step"]
                }).ToList();

            if (steps.Count > 0)
            {
                foreach (var step in steps)
                {
                    response = DeleteTestStep(issueId, step.id, projectId);
                    if (response == null) Console.WriteLine($"Delete test step \"{step.step}\" failed");
                }
            }
        }

        public IList<Execution> GetExecutionsForIssue(long issueId, long projectId, int offset, out int pageSize, out int totalCount)
        {
            var executions = GetListOfExecutions(issueId, projectId, offset);
            if (executions == null)
                throw new InvalidOperationException("Can't get executions from Zephry by API call");

            totalCount = (int)executions["totalCount"];
            if (totalCount == 0)
            {
                pageSize = 0;
                return null;
            }
            pageSize = (int)executions["maxAllowed"];

            var executionList = (from e in executions["executions"]
                                 select new Execution
                                 {
                                     CycleId = (string)e[Execution]["cycleId"],
                                     Id = (string)e[Execution]["id"],
                                     IssueId = (long)e[Execution]["issueId"],
                                     IssueKey = (string)e["issueKey"],
                                     ProjectId = (long)e[Execution]["projectId"],
                                     VersionId = (long)e[Execution]["versionId"]
                                 }).ToList();

            return executionList;
        }

        #region Private - Zapi API - https://zfjcloud.docs.apiary.io/#reference

        private JContainer GetListOfExecutions(long issueId, long projectId, int offset = 0) =>
            _zapi.Get($"{ZapiSource}/executions", $"issueId={issueId}&offset={offset}&projectId={projectId}");

        private void UpdateExecution(string executionId, UpdateExecution updateExecution) =>
            _zapi.Put($"{ZapiSource}/execution/{executionId}", updateExecution);

        private void CreateCycle(CycleCreation newCycle) =>
            _zapi.Post($"{ZapiSource}/cycle", newCycle);

        private void CloneCycle(CycleCreation newCycle, string clonedCycleId) =>
            _zapi.Post($"{ZapiSource}/cycle?clonedCycleId={clonedCycleId}&expand=executionSummaries", newCycle);

        private JContainer GetCycles(long versionId, long projectId) =>
            _zapi.Get($"{ZapiSource}/cycles/search", $"projectId={projectId}&versionId={versionId}");

        private JContainer AddTestToCycle(string cycleId, TestToCycle testToCycle) =>
            _zapi.Post($"{ZapiSource}/executions/add/cycle/{cycleId}", testToCycle);

        private JContainer CreateNewStep(long issueId, long projectId, Step testStep) =>
            _zapi.Post($"{ZapiSource}/teststep/{issueId}", $"projectId={projectId}", testStep);

        private JContainer GetAllTestSteps(long issueId, long projectId) =>
            _zapi.Get($"{ZapiSource}/teststep/{issueId}", $"projectId={projectId}");

        private JContainer DeleteTestStep(long issueId, string stepId, long projectId) =>
            _zapi.Delete($"{ZapiSource}/teststep/{issueId}/{stepId}", $"projectId={projectId}");

        #endregion
    }
}

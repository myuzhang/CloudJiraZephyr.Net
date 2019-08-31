using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZephyrCloudHelper;
using ZephyrCloudHelper.Models.Jira;

namespace TestManager
{
    public class TestSuiteManager : ITestSuiteManager
    {
        private readonly IJiraCloudApi _jiraCloudApi;
        private readonly IZephyrCloudApi _zephyrCloudApi;
        private readonly List<TestSuiteInfo> _testSuiteInfos;

        public TestSuiteManager(IJiraCloudApi jiraCloudApi, IZephyrCloudApi zephyrCloudApi)
        {
            _jiraCloudApi = jiraCloudApi;
            _zephyrCloudApi = zephyrCloudApi;

            _testSuiteInfos = new List<TestSuiteInfo>();
        }

        public async Task<TestSuiteInfo> GetTestSuiteInfo(string projectKey, string testVersion, string testCycle)
        {
            var info = _testSuiteInfos.Find(t =>
                t.Project.Key.Equals(projectKey) &&
                t.Version.Name.Equals(testVersion) &&
                t.Cycle.Name.Equals(testCycle));

            if (info != null)
                return info;

            var project = _jiraCloudApi.SetProject(projectKey);
            if (project == null)
                throw new ApplicationException($"Project \"{projectKey}\" is not found.");
            var version = _jiraCloudApi.GetVersion(testVersion);
            if (version == null)
                throw new ApplicationException($"Version \"{testVersion}\" is not found.");
            var cycle = _zephyrCloudApi.GetTestCycle(testCycle, version.Id, project.Id);
            if (cycle == null)
            {
                cycle = await CreateTestCycle(testCycle, project.Id, version.Id);
                if (cycle == null)
                    throw new ApplicationException($"Cycle \"{testCycle}\" is not created successfully.");
            }

            info = new TestSuiteInfo
            {
                Project = project,
                Version = version,
                Cycle = cycle
            };

            _testSuiteInfos.Add(info);

            return info;
        }

        // wait 10 seconds for Zephyr to add test into the test cycle
        private async Task<Cycle> CreateTestCycle(string testCycle, long projectId, long versionId)
        {
            var retry = 5;
            _zephyrCloudApi.CreateTestCycle(testCycle, projectId, versionId);

            do
            {
                var cycle = _zephyrCloudApi.GetTestCycle(testCycle, versionId, projectId);
                if (cycle != null || --retry < 0)
                    return cycle;

                await Task.Delay(TimeSpan.FromSeconds(5));
            } while (true);
        }
    }
}

using System.Collections.Generic;
using ZephyrCloudHelper.Net.Models.Zephyr;

namespace ZephyrCloudHelper.Net.Models.Jira
{
    public class IssueInCycle
    {
        public string Key { get; set; }
        public string TestSummary { get; set; }
        public string TestName { get; set; }
        public string VersionName { get; set; }
        public List<string> Labels { get; set; }
        public List<Step> Steps { get; set; }
        public List<string> CycleNames { get; set; }
        public bool Overwritten { get; set; }
    }
}

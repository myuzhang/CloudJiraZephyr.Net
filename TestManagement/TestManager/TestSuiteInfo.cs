using ZephyrCloudHelper.Models.Jira;
using Version = ZephyrCloudHelper.Models.Jira.Version;

namespace TestManager
{
    public class TestSuiteInfo
    {
        public Project Project { get; set; }
        public Version Version { get; set; }
        public Cycle Cycle { get; set; }
    }
}

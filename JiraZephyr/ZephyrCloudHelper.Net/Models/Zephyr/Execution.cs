namespace ZephyrCloudHelper.Net.Models.Zephyr
{
    /// <summary>
    /// Zephyr test execution
    /// </summary>
    public class Execution
    {
        public string Id { get; set; }

        // Jira ticket number, like CPR-1234
        public string IssueKey { get; set; }

        public long IssueId { get; set; }

        public string CycleId { get; set; }

        public long VersionId { get; set; }

        public long ProjectId { get; set; }
    }
}

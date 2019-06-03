namespace ZephyrCloudHelper.Models.Jira
{
    /// <summary>
    /// Jira issue
    /// </summary>
    public class Issue
    {
        public long Id { get; set; }

        public string Key { get; set; }

        public long ProjectId { get; set; }
    }
}

namespace ZephyrCloudHelper.Models.Jira
{
    public class Cycle
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public long ProjectId { get; set; }

        public long VersionId { get; set; }
    }
}

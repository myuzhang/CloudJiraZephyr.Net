namespace ZephyrCloudHelper.Models.Jira
{
    /// <summary>
    /// Zephyr test version
    /// </summary>
    public class Version
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public long ProjectId { get; set; }
    }
}

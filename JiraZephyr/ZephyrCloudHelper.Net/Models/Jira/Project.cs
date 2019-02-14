namespace ZephyrCloudHelper.Net.Models.Jira
{
    /// <summary>
    /// Jira project
    /// </summary>
    public class Project
    {
        public long Id { get; set; }

        public string Key { get; set; }

        public string Name { get; set; }
    }
}

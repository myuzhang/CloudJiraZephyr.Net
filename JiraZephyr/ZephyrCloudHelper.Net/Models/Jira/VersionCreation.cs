namespace ZephyrCloudHelper.Net.Models.Jira
{
    public class VersionCreation
    {
        public string description { get; set; }
        public string name { get; set; }
        public bool archived { get; set; }
        public bool released { get; set; }
        public string startDate { get; set; }
        public string project { get; set; }
        public long projectId { get; set; }
    }
}

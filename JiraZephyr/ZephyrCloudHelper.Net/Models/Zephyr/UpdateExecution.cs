namespace ZephyrCloudHelper.Net.Models.Zephyr
{
    /// <summary>
    /// Zephyr request payload to update execution
    /// </summary>
    public class UpdateExecution
    {
        public Status status { get; set; }
        public string id { get; set; }
        public long projectId { get; set; }
        public long issueId { get; set; }
        public string cycleId { get; set; }
        public long versionId { get; set; }
        // comment below 4 fields, will update the restful request to get fields back
        //public string comment { get; set; }
        //public List<string> defects { get; set; }
        //public string assigneeType { get; set; }
        //public string assignee { get; set; }
    }

    /// <summary>
    /// Zephyr test status
    /// </summary>
    public class Status
    {
        public long id { get; set; }
    }
}

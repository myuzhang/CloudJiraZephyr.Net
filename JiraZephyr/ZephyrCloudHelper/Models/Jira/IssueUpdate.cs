using System.Collections.Generic;

namespace ZephyrCloudHelper.Models.Jira
{
    public class IssueUpdate
    {
        public UpdateFields fields { get; set; }
    }

    public class UpdateFields
    {
        public string summary { get; set; }
        public string description { get; set; }
        public List<string> labels { get; set; }
    }
}

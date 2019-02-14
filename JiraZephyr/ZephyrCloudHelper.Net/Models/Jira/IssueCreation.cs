using System.Collections.Generic;

namespace ZephyrCloudHelper.Net.Models.Jira
{
    public class IssueCreation
    {
        public Fields fields { get; set; }
    }

    public class ProjectField
    {
        public string key { get; set; }
    }

    public class Issuetype
    {
        public string name { get; set; }
    }

    public class Reporter
    {
        public string name { get; set; }
    }

    // This is the custom field for 'Automated' flag
    // id = 10201 means Yes
    // id = 10200 means No
    public class Customfield10500
    {
        public string id { get; set; }
    }

    public class Fields
    {
        public ProjectField project { get; set; }
        public string summary { get; set; }
        public string description { get; set; }
        public Issuetype issuetype { get; set; }
        public Reporter reporter { get; set; }
        public Customfield10500 customfield_10500 { get; set; }
        public List<string> labels { get; set; }
    }
}

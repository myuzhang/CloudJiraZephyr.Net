namespace ZephyrCloudHelper.Net.Models
{
    /// <summary>
    /// Jira API credential and url info
    /// </summary>
    public class JiraApi
    {
        // Jira basic authentication, like "Basic xxxxxxxxxxxxxxxxxxxxxx"
        public string Authentication { get; set; }

        // Your company's domain, like https://example.atlassian.net
        public string JiraCloudUrl { get; set; }

        // The Jira product key, e.g. Example-1234 where "Example" is the project key
        public string ProjectKey { get; set; }
    }
}
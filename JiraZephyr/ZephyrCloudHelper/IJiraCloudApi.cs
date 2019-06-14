using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using ZephyrCloudHelper.Models.Jira;

namespace ZephyrCloudHelper
{
    public interface IJiraCloudApi
    {
        Project Project { get; }
        Project GetProject();
        Project GetProject(string projectKey);
        Project SetProject(string projectKey);
        IList<Version> GetVersions();
        Version GetVersion(string versionName);
        Issue GetIssue(string issueKey);
        IList<string> QueryIssueKeys(string queryString, bool exactMatch = false);
        JContainer Search(string jql, List<string> fields, int startAt = 0, int maxResults = 100);
        IList<Issue> SearchIssues(string issueSummary, bool exactMatch = false);
        Issue CreateIssue(IssueCreation issueCreation);
        void UpdateIssue(string issueKey, IssueUpdate issueUpdate);
        Version GetVersion(long versionId);
        Version CreateVersion(VersionCreation versionCreation);
    }
}
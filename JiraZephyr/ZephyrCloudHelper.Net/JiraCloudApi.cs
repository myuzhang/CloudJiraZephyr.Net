using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using ZephyrCloudHelper.Net.Models;
using ZephyrCloudHelper.Net.Models.Jira;
using Version = ZephyrCloudHelper.Net.Models.Jira.Version;

namespace ZephyrCloudHelper.Net
{
    /// <summary>
    /// Jira API related methods
    /// </summary>
    public class JiraCloudApi
    {
        private const string Id = "id";
        private const string Key = "key";
        private const string Name = "name";
        private const string ProjectId = "projectId";

        private const string JiraSource = "/rest/api/2";


        private readonly RestClient _restClient;
        private readonly JiraApi _jiraApi;

        private Project _project;

        public JiraCloudApi(JiraApi jiraApi)
        {
            _jiraApi = jiraApi;
            _restClient = new RestClient(jiraApi.JiraCloudUrl);
            _restClient.AddDefaultHeader("Authorization", jiraApi.Authentication);
        }

        public Project Project => _project ?? (_project = GetProject());

        public Project GetProject()
        {
            var source = $"{JiraSource}/project/{_jiraApi.ProjectKey}";
            var response = SendHttpRequest(source, Method.GET);
            return new Project
            {
                Id = (long)response[Id],
                Key = (string)response[Key],
                Name = (string)response[Name]
            };
        }

        public IList<Version> GetVersions()
        {
            var source = $"{JiraSource}/project/{_jiraApi.ProjectKey}/version?maxResults=500&orderBy=-sequence";
            var response = SendHttpRequest(source, Method.GET);
            if (response == null) return null;
            var jsonVersions = (JArray) response["values"];
            return (from v in jsonVersions
                select new Version
                {
                    Id = (long) v[Id],
                    Name = (string) v[Name],
                    ProjectId = (long) v[ProjectId]
                }).ToList();
        }

        public Version GetVersion(string versionName)
        {
            var version = GetVersions()
                .FirstOrDefault(v => v.Name.Equals(versionName, StringComparison.CurrentCultureIgnoreCase));

            return version;
        }

        public Issue GetIssue(string issueKey)
        {
            var source = $"{JiraSource}/issue/{issueKey}";
            var response = SendHttpRequest(source, Method.GET);
            if (response == null) return null;
            return new Issue
            {
                Id = (long)response[Id],
                Key = (string)response[Key],
                ProjectId = (long)response["fields"]["project"][Id]
            };
        }

        public IList<string> QueryIssueKeys(string queryString, bool exactMatch = false)
        {
            List<string> keys = null;
            var source = $"{JiraSource}/issue/picker?query={queryString}";
            var response = SendHttpRequest(source, Method.GET);
            if (response == null) return null;
            var sectionIssues = (from s in response["sections"][0]["issues"] as JArray
                select new
                {
                    Key = (string) s["key"],
                    Summary = (string) s["summaryText"]
                }).ToList();

            if (exactMatch)
            {
                if (sectionIssues.Count > 0)
                {
                    keys =
                        sectionIssues.
                            Where(s => queryString.Trim().
                                Equals(s.Summary?.Trim(), StringComparison.CurrentCultureIgnoreCase)).
                            Select(s => s.Key).
                            ToList();
                }
            }
            else
            {
                keys = sectionIssues.Select(s => s.Key).ToList();
            }

            return keys;
        }

        public Issue CreateIssue(IssueCreation issueCreation)
        {
            var source = $"{JiraSource}/issue";
            var response = SendHttpRequest(source, Method.POST, issueCreation);
            if (response == null) return null;
            return new Issue
            {
                Id = (long)response[Id],
                Key = (string)response[Key],
                ProjectId = Project.Id
            };
        }

        public void UpdateIssue(string issueKey, IssueUpdate issueUpdate)
        {
            var source = $"{JiraSource}/issue/{issueKey}";
            SendHttpRequest(source, Method.PUT, issueUpdate);
        }

        public Version GetVersion(long versionId)
        {
            var source = $"{JiraSource}/version/{versionId}";
            var response = SendHttpRequest(source, Method.GET);
            if (response == null) return null;
            return new Version
            {
                Id = (long)response[Id],
                Name = (string)response[Name],
                ProjectId = (long)response[ProjectId]
            };
        }

        public Version CreateVersion(VersionCreation versionCreation)
        {
            var source = $"{JiraSource}/version";
            var response = SendHttpRequest(source, Method.POST, versionCreation);
            if (response == null) return null;
            return new Version
            {
                Id = (long)response[Id],
                Name = (string)response[Name],
                ProjectId = (long)response[ProjectId]
            };
        }

        private JContainer SendHttpRequest(string source, Method method, object requestPayload = null)
        {
            IRestResponse response = null;
            try
            {
                var request = new RestRequest(source, method);
                if (method == Method.POST || method == Method.PUT)
                {
                    var json = JsonConvert.SerializeObject(requestPayload, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
                    request.AddParameter("application/json", json, ParameterType.RequestBody);
                }

                response = _restClient.Execute(request);
                if (response.StatusCode.Equals(HttpStatusCode.NotFound))
                    return null;

                var data = (JContainer)JToken.Parse(response.Content);
                return data;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Jira - The http status is:{response?.StatusCode}\nThe http content is:{response?.Content}\\nThe exception is:{e.Message}");
                return null;
            }
        }
    }
}

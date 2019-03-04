# Jira

The repo is for Jira or other Atlanssian products related.
You can find the nuget package here - https://www.nuget.org/packages/ZephyrCloudHelper.Net/

## Prerequisite:
* You need both Jira token and Zephyr token
* Your Jira url, e.g. https://organization.atlassian.net
* The Zephyr url, as this is for cloud the url is always this: "https://prod-api.zephyr4jiracloud.com/[organization]" - you can still try it for on-prem

## How to use:
1. Use helper class: TestManagementHelper, this class provides two methods that you can update test case result and create test case
  * Pass two objects Zapi and JiraApi
  * Zapi needs Zephyr's user name, access key and scret key which you can find from the Jira -> Zephyr on your Jira website
  * JiraApi needs basic authentication, your organization's jira url and project key, and you can find these info from your Jira website
  
2. Use service class: AtlassianService, this class provides more actions, it also requres objects Zapi and JiraApi

## To users:
I am glad if you like to use it. If you find any issues please free to contact me and I will fix it. Also welcome to adding more Jira/Zephyr actions to this repository.

There is no unit tests for this solution as I can't provide my organization's data for testing here.

# Jira

We have two implementations for Jira/Zephyr:
* JiraZephyr - it provides a set of API to interact with Jira/Zephyr and there is nuget package available - https://www.nuget.org/packages/ZephyrCloudHelper.Net/. 
* TestManagement - it gives examples on how to use JiraZephyr 


## JiraZephyr

It is implemented in folder 'JiraZephyr'.

### Prerequisite:
* You need both Jira token and Zephyr token
* Your Jira url, e.g. https://organization.atlassian.net
* The Zephyr url, as this is for cloud the url is always this: "https://prod-api.zephyr4jiracloud.com/[organization]" - you can still try it for on-prem

### Projects:
We have two projects, they are doing the same thing but different framework
  1. ZephyrCloudHelper is .net standard 2.0
  2. ZephyrCloudHelper.net is .net framework 4.6

### How to use:
1. Use helper class: TestManagementHelper, this class provides two methods that you can update test case result and create test case
  * Pass two objects Zapi and JiraApi
  * Zapi needs Zephyr's user name, access key and scret key which you can find from the Jira -> Zephyr on your Jira website
  * JiraApi needs basic authentication, your organization's jira url and project key, and you can find these info from your Jira website
  
2. Use service class: AtlassianService, this class provides some basic actions, it also requres objects Zapi and JiraApi

3. If you are using project ZephyrCloudHelper, there are interface IJiraCloudApi and IZephyrCloudApi for you to use and they provide more actions on Jira/Zephyr


## TestManagement

It is implemented in folder 'TestManagement' and we will have three use cases:
* AWS SQS to Lambda - achieved updating test result
* AWS API Gateway to Lamba - in plan
* Docker container - in developing

### AWS SQS to Lambda

This is applied when you like to manage your test cases and execution result via SQS.

The SQS message format is:
```
{
	"TestKey": "test key where you can get it from url, if this one is not provided, Title will be used to search jira ticket"
	"Title": "test title, if TestKey is not provided, Title will be used to find the jira ticket",
	"Description": "test description, if you put empty string here, no description will be created in the Jira test case, whic is optional",
	"Result": "Passed or Failed, which is required",
	"TestCycle": "test cycle name, which is required",
	"TestVersion": "test version name, which is required",
	"ProjectKey": "project key, which is required",
}
```

After you post the message to SQS, it does test management and execution result update for you:
  1. looking for project key (failed if not found)
  2. looking for test version (failed if not found)
  3. looking for test cycle (automatically create a new one if not found)
  4. looking for test case in Jira (automatically create a new one if not found)
  5. looking for test execution in Zephyr (automatically assigned the test case to version/cycle if not found)
  6. update test execution
  
According to the 'test management and execution result update', what you need to do is to manually create release in Jira (which will be as TestVersion in Zephyr) and you are done. Lambda will manage test cases and execution result for you.

#### How to use:
1. Create a Lambda function
2. Create a SQS and set it as the triger event to Lambda function
3. Compile the project and zip all dlls and upload to the Lambda
4. set environemnt variable in Lambda: jiraAuthentication, jiraJiraCloudUrl, jiraProjectKey, zephyrAccessKey, zephyrSecretKey, zephyrUser
5. send SQS message to the SQS

### AWS API Gateway to Lamba

This is pretty much the same as 'AWS SQS to Lambda', but use you need to deploy to use API Gateway as a event trigger.

### Docker container

The test management and test result update is the same as 'AWS SQS to Lambda', it is using docker container though.
Run command to launch the test manager:
`docker run -d -p 9000:80 myuzhang/cloud-jira-zephyr:1.0.0`

You can add this as a deployment step in your CI process.

Please go to https://cloud.docker.com/repository/registry-1.docker.io/myuzhang/cloud-jira-zephyr for details.

## To users:
I am glad if you like to use it. If you find any issues please free to contact me and I will fix it. Also welcome to adding more Jira/Zephyr actions to this repository.

There is no unit tests for this solution as I can't provide my organization's data for testing here.

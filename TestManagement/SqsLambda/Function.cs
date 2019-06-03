using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using ZephyrCloudHelper;
using ZephyrCloudHelper.Models;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SqsLambda
{
    public class Function
    {
        private ServiceProvider _provider;

        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {
            BuildService();
        }

        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
        /// to respond to SQS messages.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            foreach(var message in evnt.Records)
            {
                await ProcessMessageAsync(message, context);
            }
        }

        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            context.Logger.LogLine($"Processed sqs message: {message.Body}");
            try
            {
                var testInfo = JsonConvert.DeserializeObject<TestInfo>(message.Body);
                context.Logger.LogLine($"Updating test : \"{testInfo.Title}\" under \"{testInfo.ProjectKey}\" project in release \"{testInfo.TestVersion}\" and test cycle is \"{testInfo.TestCycle}\"");

                var (result, jiraTicket) = await UniTask(testInfo);

                var success = result ? "successfully" : "unsuccessfully";
                context.Logger.LogLine($"Updated test result for \"{jiraTicket} {testInfo.Title}\" {success}");

                //await Task.CompletedTask;
            }
            catch (Exception e)
            {
                context.Logger.LogLine($"Failed to update test result for:\n {message.Body}\n The reason:\n {e.Message}");
            }
        }

        public async Task<(bool, string)> UniTask(TestInfo testInfo)
        {
            var manager = _provider.GetRequiredService<IExecutionManager>();
            return await manager.UpdateTestResult(testInfo);
        }

        private void BuildService()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IJiraCloudApi, JiraCloudApi>(sp => new JiraCloudApi(new JiraApi
            {
                Authentication = Environment.GetEnvironmentVariable("jiraAuthentication"),
                JiraCloudUrl = Environment.GetEnvironmentVariable("jiraJiraCloudUrl"),
                ProjectKey = Environment.GetEnvironmentVariable("jiraProjectKey")
            }));

            services.AddSingleton<IZephyrCloudApi, ZephyrCloudApi>(sp => new ZephyrCloudApi(new Zapi
            {
                AccessKey = Environment.GetEnvironmentVariable("zephyrAccessKey"),
                SecretKey = Environment.GetEnvironmentVariable("zephyrSecretKey"),
                User = Environment.GetEnvironmentVariable("zephyrUser"),
            }));

            services.AddSingleton<ITestSuiteManager, TestSuiteManager>();
            services.AddTransient<IExecutionManager, ExecutionManager>();

            _provider = services.BuildServiceProvider();
        }
    }
}

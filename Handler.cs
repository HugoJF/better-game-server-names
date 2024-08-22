using System;
using System.Collections.Generic;
using System.Net;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;
using OpenAI.Chat;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(
    typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AwsDotnetCsharp
{
    public class Handler
    {
        private const string PromptFormat = "Summary the following Battlefield gameserver names into a shorter version, removing phones, links, anti-cheat, information about VIP, keeping only information that is related to gameplay itself, just output the result. You can keep characters that are used for decoration: {0}";

        public APIGatewayProxyResponse HelloWorld(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            ChatClient client = new(model: "gpt-4o-mini", apiKey);
            
            request.QueryStringParameters.TryGetValue("servername", out var servername);
            var prompt = string.Format(PromptFormat, servername);
            try
            {
                ChatCompletion completion = client.CompleteChat(prompt);

                return CreateResponse(new Dictionary<string, string>()
                    {
                        { "prompt", prompt },
                        { "response", completion.ToString() }
                    }
                );
            }
            catch (Exception e)
            {
                return CreateResponse(new Dictionary<string, string>()
                {
                    { "error", e.Message }
                });
            }
        }

        private static APIGatewayProxyResponse CreateResponse(object result)
        {
            var statusCode = (result != null) ? (int)HttpStatusCode.OK : (int)HttpStatusCode.InternalServerError;

            var body = (result != null) ? JsonConvert.SerializeObject(result) : string.Empty;

            var response = new APIGatewayProxyResponse
            {
                StatusCode = statusCode,
                Body = body,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" }
                }
            };

            return response;
        }
    }
}
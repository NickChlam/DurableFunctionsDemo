using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.AspNetCore.Http.Extensions;
using System.Linq;
using System.Web.Http;

namespace VideoProcessor
{
    public static class ProcessVideoStarter
    {
        [FunctionName("ProcessVideoStarter")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] 
            HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            // parse query params
            string video =  req.Query["video"];

            // get body request 
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            video = video ?? data?.video;

            if (video == null)
            {
                return new BadRequestErrorMessageResult("Please pass a video location in query or body");
            }

            log.LogInformation($"About to start orch for {video}");

            // provide a function name returns an instance id - 
            var orchestrationId = await starter.StartNewAsync<string>("O_ProcessVideo", video);

            // takes the request object and orch id and works out he urls to check progress of orchstration
            return starter.CreateCheckStatusResponse(req, orchestrationId);


        }
    }
}

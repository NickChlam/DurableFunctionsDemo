using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoProcessor
{
    public static class ProcessVideoOrchetrators
    {
        [FunctionName("O_ProcessVideo")]
        public static async Task<object> ProcessVideo(
            [OrchestrationTrigger] IDurableOrchestrationContext ctx, ILogger log)
        {
            var videoLocation = ctx.GetInput<string>();

            var bitRates = await ctx.CallActivityAsync<int[]>("A_GetBitRates", null);
            var transCodeTasks = new List<Task<VideoFileInfo>>();

            foreach (var rate in bitRates)
            {
                var info = new VideoFileInfo { BitRate = rate, Location = videoLocation };
                var task = ctx.CallActivityAsync<VideoFileInfo>("A_TranscodeVideo", info);
                transCodeTasks.Add(task);
            }

            if (!ctx.IsReplaying)
                log.LogInformation("About to call transcode activity");
            // take activitys as inputs and outputs - take a location of the file and return a string as output
            var transcodeResults = await Task.WhenAll(transCodeTasks);
            var transcodedLocation = transcodeResults
                .OrderByDescending(r => r.BitRate)
                .Select(r => r.Location)
                .First();


            if (!ctx.IsReplaying)
                log.LogInformation("About to call ThumbNail activity");
            var thumbnailLocation = await
                ctx.CallActivityAsync<string>("A_ExtractThumbnail", transcodedLocation);

            if (!ctx.IsReplaying)
                log.LogInformation("About to call Intro activity");
            var withIntroLocation = await
                ctx.CallActivityAsync<string>("A_PrependIntro", transcodedLocation);

            return new
            {
                Transcoded = transcodedLocation,
                Thumbnail = thumbnailLocation,
                WithIntro = withIntroLocation
            };
        }
    }
}

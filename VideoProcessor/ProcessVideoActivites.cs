using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace VideoProcessor
{
    

    public static class ProcessVideoActivites
    {


        [FunctionName("A_GetBitRates")]
        public static int[] GetBitRates(
            [ActivityTrigger] object input,
            ExecutionContext context,
            ILogger log)
        {
            log.LogInformation($"Getting BitRates");

            // TODO move out into helper class
            var config = new ConfigurationBuilder()
             .SetBasePath(context.FunctionAppDirectory)
             .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
             .AddEnvironmentVariables()
             .Build();

            var bitRates = config["BitRates"];

            return bitRates
                .Split(',')
                .Select(int.Parse)
                .ToArray();

        }

            

        [FunctionName("A_TranscodeVideo")]
        public static async Task<VideoFileInfo> TranscodeVideo(
            [ActivityTrigger] VideoFileInfo inputVideo,
            ILogger log)
        {
            log.LogInformation($"Transcoding {inputVideo.Location} to: {inputVideo.BitRate}");

            await Task.Delay(5000);

            var transcodedLocation = $"{Path.GetFileNameWithoutExtension(inputVideo.Location)}--" + $"{inputVideo.BitRate}kbps.mp4";

            return new VideoFileInfo
            {
                Location = transcodedLocation,
                BitRate = inputVideo.BitRate
            };
        }

        [FunctionName("A_ExtractThumbnail")]
        public static async Task<string> ExtractThumbnail(
            [ActivityTrigger] string inputVideo,
            ILogger log)
        {
            log.LogInformation($"Extracting Thumbnail {inputVideo}");

            await Task.Delay(5000);

            return "thumbnail.mp4";

        }

        [FunctionName("A_PrependIntro")]
        public static async Task<string> PrependIntro(
           [ActivityTrigger] string inputVideo,
            ExecutionContext context,
           ILogger log)
        {
            var config = new ConfigurationBuilder()
              .SetBasePath(context.FunctionAppDirectory)
              .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables()
              .Build();

            log.LogInformation($"Appending intro to video {inputVideo}");
            var introLocation = config["IntroLocation"];

            log.LogInformation($"Intro Location: {introLocation}");

            await Task.Delay(5000);

            return "withIntro.mp4";

        }

    }
}

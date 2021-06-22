using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using Web_Api.Models;
using System.Collections.Generic;

namespace CreateStatisticsNew
{
    public static class HTTPFunction
    {
        [FunctionName("HTTPFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log, [Table("Statistik",  Connection = "AzureWebJobsStorage")] CloudTable cloudtable)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            TableContinuationToken token = null;
            var entities = new List<Statistics>();
            do
            {
                var queryResult = await cloudtable.ExecuteQuerySegmentedAsync(new TableQuery<Statistics>(), token);
                entities.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            return new OkObjectResult(entities);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace SendEmailToReaders
{
    public static class StoreEmail
    {
        [FunctionName("StoreEmail")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {

            var postData = await req.Content.ReadAsFormDataAsync();
            var missingFields = new List<string>();
            if (postData["fromEmail"] == null)
            {
                missingFields.Add("fromEmail");
            }

            if (missingFields.Any())
            {
                var missingFieldsSummary = String.Join(", ", missingFields);
                return req.CreateResponse(HttpStatusCode.BadRequest, $"Missing field(s): {missingFieldsSummary}");
            }

            try
            {

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["TableStorageConnString"]);

                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                CloudTable table = tableClient.GetTableReference("MCBlogSubscribers");

                table.CreateIfNotExists();

                CreateMessage(table, new EmailEntity(postData["fromEmail"], false));

                return req.CreateResponse(HttpStatusCode.OK, "Thanks! I've successfully received your request. "); //
            }
            catch (Exception ex)
            {
                return req.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    status = false,
                    message = $"There are problems storing your email address: {ex.GetType()}"
                });
            }

        }

        static void CreateMessage(CloudTable table, EmailEntity newemail)
        {
            TableOperation insert = TableOperation.Insert(newemail);

            table.Execute(insert);
        }
    }

  
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.ServiceModel.Syndication;
using System.Linq;
using System.Configuration;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using SendGrid.SmtpApi;

namespace SendEmailToReaders
{
    public static class SendEmail
    {
        [FunctionName("SendEmail")]
        public static async Task Run([TimerTrigger("0 30 9 * * *")]TimerInfo myTimer, TraceWriter log)
        {

            string feedurl = "https://www.michaelcrump.net/feed.xml";
            string last7days = "";

            XmlReader reader = XmlReader.Create(feedurl);
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            reader.Close();
            foreach (SyndicationItem item in feed.Items)
            {
                if ((DateTime.Now - item.PublishDate).TotalDays < 7)
                {
                    last7days = last7days + "<a href=\"" + item.Links[0].Uri + "\">" + item.Title.Text + "</a><br>";
                }       
            }

            var client = new SendGridClient(ConfigurationManager.AppSettings["SendGridAPIKey"]);
            var msg = new SendGridMessage();

            msg.SetFrom(new EmailAddress("michael@michaelcrump.net", "Michael Crump Blog"));

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["TableStorageConnString"]);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("MCBlogSubscribers");
            table.CreateIfNotExists();

            var header = new Header();
            List<EmailAddress> recipientlist = GetAllEmailAddresses(table);

            msg.AddTos(recipientlist);
            msg.SetSubject("Weekly Digest for MichaelCrump.net Blog");

            msg.AddContent(MimeType.Html, last7days);
            //TODO: needs to have a header so it doesn't show the list of email addresses
            //msg.Headers.Add("X-SMTPAPI", header.JsonString());
            var response = await client.SendEmailAsync(msg);

        }

        public static List<EmailAddress> GetAllEmailAddresses(CloudTable table)
        {
            var retList = new List<EmailAddress>();

            TableQuery<EmailEntity> query = new TableQuery<EmailEntity>()
                    .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "SendEmailToReaders"));

            foreach (EmailEntity emailname in table.ExecuteQuery(query))
            {
                retList.Add(new EmailAddress(emailname.EmailAddress));
            }

            return retList;
        }
    }
}

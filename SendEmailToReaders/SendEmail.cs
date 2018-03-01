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
using System.Net.Mail;
using System.Text;

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

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["TableStorageConnString"]);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("MCBlogSubscribers");
            table.CreateIfNotExists();

            var header = new Header();

            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.Host = "smtp.sendgrid.net";
            client.Timeout = 10000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["SendGridUserName"], ConfigurationManager.AppSettings["SendGridSecret"]);

            MailMessage mail = new MailMessage();
            List<string> recipientlist = GetAllEmailAddresses(table);
            header.SetTo(recipientlist);
            mail.From = new MailAddress("michael@michaelcrump.net", "Azure Tips and Tricks");
            mail.To.Add("no-reply@michaelcrump.net");
            mail.Subject = "Weekly Digest for MichaelCrump.net Blog";
            mail.BodyEncoding = Encoding.UTF8;
            mail.SubjectEncoding = Encoding.UTF8;

            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(last7days);
            htmlView.ContentType = new System.Net.Mime.ContentType("text/html");
            mail.AlternateViews.Add(htmlView);
            mail.Body = "Please enable HTML in order to view the message";

            mail.Headers.Add("X-SMTPAPI", header.JsonString());

            await client.SendMailAsync(mail);

            mail.Dispose();

        }

        public static List<string> GetAllEmailAddresses(CloudTable table)
        {
            var retList = new List<string>();

            TableQuery<EmailEntity> query = new TableQuery<EmailEntity>()
                    .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "SendEmailToReaders"));

            foreach (EmailEntity emailname in table.ExecuteQuery(query))
            {
                retList.Add(emailname.EmailAddress);
            }

            return retList;
        }
    }
}

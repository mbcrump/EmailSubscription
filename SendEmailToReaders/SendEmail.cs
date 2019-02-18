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
        public static async Task Run([TimerTrigger("0 30 9 * * SUN")]TimerInfo myTimer, TraceWriter log)
       // public static async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, TraceWriter log)
        {

            string feedurl = "https://www.michaelcrump.net/feed.xml";
            string last7days = "";

            XmlReader reader = XmlReader.Create(feedurl);
            SyndicationFeed feed = SyndicationFeed.Load(reader);


            last7days = last7days + "<b>New updates in the last 7 days:</b><br><br>";
            foreach (SyndicationItem item in feed.Items)
            {
                if ((DateTime.Now - item.PublishDate).TotalDays < 8)
                {
                    last7days = last7days + "<a href=\"" + item.Links[0].Uri + "\">" + item.Title.Text + "</a><br>";
                }
            }


            reader.Close();

            feedurl = "https://podsync.net/Z1F8nb1tN";
            //original youtube playlist feed url
            //feedurl = "https://www.youtube.com/feeds/videos.xml?playlist_id=PLLasX02E8BPCNCK8Thcxu-Y-XcBUbhFWC";
            XmlReader reader1 = XmlReader.Create(feedurl);
            SyndicationFeed feed1 = SyndicationFeed.Load(reader1);
            
            
            foreach (SyndicationItem item in feed1.Items)
            {
                if ((DateTime.Now - item.PublishDate).TotalDays < 8)
                {
                    last7days = last7days + "<a href=\"" + item.Links[0].Uri + "\">" + "[New Video available] - " + item.Title.Text + "</a><br>";
                }
            }

            reader1.Close();

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
            //comment out for prod
            //List<string> recipientlist = new List<string>();
            //recipientlist.Add("mbcrump29@gmail.com");
            //
            header.SetTo(recipientlist);
            mail.From = new MailAddress("michael@michaelcrump.net", "MichaelCrump.net");
            mail.To.Add("no-reply@michaelcrump.net");
            mail.Subject = "Weekly Digest for MichaelCrump.net Blog and Azure Tips and Tricks";
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

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using SendGrid.SmtpApi;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Mail;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SendEmailToReaders
{           //dfasdf
    public static class SendEmail
    {
        [FunctionName("SendEmail")]
       public static async Task Run([TimerTrigger("0 30 9 * * SUN")]TimerInfo myTimer, TraceWriter log)
        //public static async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, TraceWriter log)
        {


            //string feedurl = "https://www.michaelcrump.net/feed.xml";
            string feedurl = "https://microsoft.github.io/AzureTipsAndTricks/rss.xml";
            string last7days = "";

            XmlReader reader = XmlReader.Create(feedurl);
            SyndicationFeed feed = SyndicationFeed.Load(reader);


            last7days = last7days + "<b>Latest updates in the past 7 days:</b><br><br>";
            foreach (SyndicationItem item in feed.Items)
            {
                if ((DateTime.Now - item.PublishDate).TotalDays < 8)
                {
                    last7days = last7days + "<a href=\"" + item.Links[0].Uri + "\">" + "Azure Tips and Tricks: " + item.Title.Text + "</a><br>";
                }
            }


            reader.Close();


            feedurl = "http://fetchrss.com/rss/5cb90f018a93f83d098b45675cb90ed88a93f829098b4567.xml";
            //original youtube playlist feed url
            //feedurl = "https://www.youtube.com/feeds/videos.xml?playlist_id=PLLasX02E8BPCNCK8Thcxu-Y-XcBUbhFWC";
            XmlReader reader1 = XmlReader.Create(feedurl);
            SyndicationFeed feed1 = SyndicationFeed.Load(reader1);


            foreach (SyndicationItem item in feed1.Items)
            {
                if ((DateTime.Now - item.PublishDate).TotalDays < 8)
                {
                    last7days = last7days + "<a href=\"" + item.Links[0].Uri + "\">" + "Azure Tips and Tricks Video: " + item.Title.Text.Replace(" | Azure Tips and Tricks", "") + " </a><br>";
                }
            }

            reader1.Close();

            feedurl = "http://fetchrss.com/rss/5cb90f018a93f83d098b45675deaae3f8a93f8ac308b4567.xml";
            XmlReader reader2 = XmlReader.Create(feedurl);
            SyndicationFeed feed2 = SyndicationFeed.Load(reader2);


            foreach (SyndicationItem item in feed2.Items)
            {
                if ((DateTime.Now - item.PublishDate).TotalDays < 8)
                {
                    last7days = last7days + "<a href=\"" + item.Links[0].Uri + "\">" + "Azure Developer Streams Video: " + item.Title.Text.Replace(" | Azure Developer Streams", "") + " </a><br>";
                }
            }

            reader2.Close();

            last7days = last7days + "<br><br>Follow me on <a href=\"https://twitter.com/mbcrump\">" + "Twitter</a> for daily developer news or on " + "<a href=\"https://twitch.tv/mbcrump\">" + "Twitch</a> for live coding sessions";

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

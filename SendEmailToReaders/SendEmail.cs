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

            var recipients = new List<EmailAddress>
                {
                    new EmailAddress("mbcrump29@gmail.com")//,
                    //new EmailAddress("more email addresses"),
                    //new EmailAddress("c@gmail.com")
                };
            msg.AddTos(recipients);

            msg.SetSubject("Weekly Digest for MichaelCrump.net Blog");

            msg.AddContent(MimeType.Html, last7days);
            var response = await client.SendEmailAsync(msg);

        }
    }
}

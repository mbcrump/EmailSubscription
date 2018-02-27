using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SendEmailToReaders
{
    class EmailEntity : TableEntity
    {

        public string EmailAddress { get; set; }
        public bool Unsubscribe { get; set; }

        public EmailEntity(string email, bool unsub)
        {
            EmailAddress = email;
            Unsubscribe = unsub;
            PartitionKey = "SendEmailToReaders";
            RowKey = Guid.NewGuid().ToString();
        }

        public EmailEntity()
        {

        }
    }
}

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

        public EmailEntity(string email)
        {
            EmailAddress = email;
            PartitionKey = "SendEmailToReaders";
            RowKey = Guid.NewGuid().ToString();
        }

        public EmailEntity()
        {

        }
    }
}

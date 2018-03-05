## Generate a weekly digest email for a blog using Azure Functions, SendGrid and Azure Table Storage. 

by: [Michael Crump](http://twitter.com/mbcrump)

* [Part 1 - What we're going to build and how to build it](http://www.michaelcrump.net/azure-tips-and-tricks97/)
* [Part 2 - Storing Emails using Azure Table Storage](http://www.michaelcrump.net/azure-tips-and-tricks98/)
* [Part 3 - Writing the Frontend with HTML5 and jQuery](http://www.michaelcrump.net/azure-tips-and-tricks99/)
* [Part 4 - Sending Emails with Sendgrid and Azure Functions](http://www.michaelcrump.net/azure-tips-and-tricks100/)

Below is a demo:

![image](https://github.com/mbcrump/EmailSubscription/blob/master/demo.gif)

## Technology Stack

After poking around for a bit, I landed on the following:

* [SendGrid](https://sendgrid.com/) to handle emails (25K emails free monthly)
* [Azure Storage Table](https://azure.microsoft.com/en-us/services/storage/) to save the email address the user enter (this gives me an unlimited number of subscribers). 
* [Timer Trigger with Azure Functions](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-timer) to schedule emails to send at a certain time. (Runs weekly and retrieves my last 7 days worth of blog posts)
* [HTTP Trigger with Azure Functions](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-http-webhook) to collect POST data coming from my website that contains the email address that someone types in. 
* [Visual Studio and the C# Language](https://www.visualstudio.com/) 

## Contact info

[Twitter - DMs open](http://twitter.com/mbcrump)

[Blog](https://www.michaelcrump.net)

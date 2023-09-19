using System;
using System.IO;
using System.Net.Mail;
using System.Net;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Azure.Storage;

namespace Azure.Blob.Trigger.Function
{
    public class Function1
    {
        [FunctionName("Function1")]
        public void Run([BlobTrigger("filestorage/{name}", Connection = "AzureWebJobsStorage")]Stream myBlob, string name, string toAddress, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            var blobServiceClient = new BlobServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            var blobContainerClient = blobServiceClient.GetBlobContainerClient("filestorage");
            var blobClient = blobContainerClient.GetBlobClient(name);

            var sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = blobContainerClient.Name,
                BlobName = blobClient.Name,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1) // Valid for 1 hour
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(blobServiceClient.AccountName, Environment.GetEnvironmentVariable("AzureWebJobsStorage"))).ToString();
            var blobUrlWithSas = $"{blobClient.Uri}?{sasToken}";

            string fromAddress = "filestoragemailadress@gmail.com";            
            string subject = "Successful";
            string body = $"Your URL -> {blobUrlWithSas}";

            //string password = "admin1vp1";

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(fromAddress, "zhjckfdkelxfr123");

            //MailMessage mailMessage = new MailMessage(fromAddress, toAddress, subject, body);

            //smtpClient.Send(mailMessage);           
        }
    }
}

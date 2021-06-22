using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Web_Api.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Cosmos;
using User = Web_Api.Models.User;

namespace CreateStatisticsNew
{
    public static class TimeFunction
    {
        [FunctionName("TimeFunction")]
        [return: Table("Statistik", Connection = "AzureWebJobsStorage")]
        public static Statistics Run([TimerTrigger("* * * * * 0")]TimerInfo myTimer, ILogger log, [CosmosDB(
        databaseName: "ccstandarddb",
        collectionName: "books",
        ConnectionStringSetting = "ConnectionStrings:Test",
        SqlQuery = "select * from books")] IEnumerable<Book> books, [CosmosDB(
        databaseName: "ccstandarddb",
        collectionName: "users",
        ConnectionStringSetting = "ConnectionStrings:Test",
        SqlQuery = "select * from users")] IEnumerable<User>  users)
        {

            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            int countBooks = 0;
            int countUsers = 0;

            Hashtable authors = new Hashtable();

            foreach (Book book in books)
            {
                log.LogInformation(book.id);
                countBooks++;
                if (!authors.Contains(book.Autor))
                    authors.Add(book.Autor, 1);
            }

            foreach (User user in users)
            {
                log.LogInformation(user.id);
                countUsers++;
            }

            return new Statistics {
                PartitionKey = DateTime.Now.DayOfYear.ToString(),
                RowKey = Guid.NewGuid().ToString(),
                AnzahlBuecher = countBooks.ToString(),
                AnzahlAutoren = authors.Count.ToString(),
                AnzahlBenutzer = countUsers.ToString()
            };
        }
    }
}

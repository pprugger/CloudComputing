using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Newtonsoft.Json;
using Web_Api.Models;
using Microsoft.Azure.Cosmos;

namespace Web_Api.Threads
{
    public class BookQueue
    {
        QueueClient queue;
        string QueueName = "bookinputqueue";
        private CosmosClient _cosmosClient;
        private Database _database;
        private Container _container;

        public BookQueue(IConfiguration configuration)
        {

            CreateClientAndDatabase(configuration);
        }

        private void CreateClientAndDatabase(IConfiguration configuration)
        {
            //Init queue

            queue = new QueueClient(configuration.GetConnectionString("AzureStorageConnect"), QueueName);
            queue.CreateIfNotExists();
            //Init CosmosDB
            _cosmosClient = new CosmosClient(configuration.GetConnectionString("CosmosDBString")); ;
            _cosmosClient.CreateDatabaseIfNotExistsAsync("ccstandarddb");
            _database = _cosmosClient.GetDatabase("ccstandarddb");
            _database.CreateContainerIfNotExistsAsync("books", "/id");
            _container = _cosmosClient.GetContainer("ccstandarddb", "books");
        }

        public async void run()
        {
            while(true)
            {
                foreach (QueueMessage message in queue.ReceiveMessages(maxMessages: 10).Value)
                {
                    Book newBook = JsonConvert.DeserializeObject<Book>(message.Body.ToString());

                    try
                    {
                        var item =  await _container.CreateItemAsync<Book>(newBook new PartitionKey(newBook.ISBN));
                        // Log message to console
                        Console.WriteLine($"Message: {message.Body}");

                        // Let the service know we're finished with the message and
                        // it can be safely deleted.
                        queue.DeleteMessage(message.MessageId, message.PopReceipt);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Adding book failed with error: {e}");
                        throw;
                    }
                }
            }
        }
    }
}

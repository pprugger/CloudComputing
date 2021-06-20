﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Web_Api.Models;
using Microsoft.Azure.Cosmos;

namespace Web_Api.Threads
{
    public class UserQueue
    {
        QueueClient queue;
        string QueueName = "userinputqueue";
        private CosmosClient _cosmosClient;
        private Database _database;
        private Container _container;

        public UserQueue(IConfiguration configuration)
        {

            CreateClientAndDatabase(configuration);
        }

        private void CreateClientAndDatabase(IConfiguration configuration)
        {
            //Init queue
            queue = new QueueClient(configuration.GetConnectionString("AzureStorageConnect"), QueueName);
            queue.CreateIfNotExists();

            //Init cosmosdb
            _cosmosClient = new CosmosClient(configuration.GetConnectionString("CosmosDBString")); ;

            _cosmosClient.CreateDatabaseIfNotExistsAsync("ccstandarddb");
            _database = _cosmosClient.GetDatabase("ccstandarddb");
            _database.CreateContainerIfNotExistsAsync("users", "/id");
            _container = _cosmosClient.GetContainer("ccstandarddb", "users");

        }

        public void run()
        {
            while(true)
            {
                foreach (QueueMessage message in queue.ReceiveMessages(maxMessages: 10).Value)
                {
                    // "Process" the message
                    Console.WriteLine($"Message: {message.Body}");

                    // Let the service know we're finished with the message and
                    // it can be safely deleted.
                    queue.DeleteMessage(message.MessageId, message.PopReceipt);
                }
            }
        }
    }
}

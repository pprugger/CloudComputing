using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Web_Api.Models;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Web_Api.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class booksController : ControllerBase
    {
        private readonly ILogger<booksController> _logger;
        private CosmosClient _cosmosClient;
        private Database _database;
        private Container _container;

        private string containerName = "images";
        private string bookQueueName = "bookinputqueue";
        private BlobServiceClient blobServiceClient;
        private BlobContainerClient containerClient;
        private QueueClient queue;

        private CloudStorageAccount storageAccount;
        private CloudTableClient tableClient;
        private string tableName = "Statistik2";
        private CloudTable table;

        public booksController(ILogger<booksController> logger, IConfiguration configuration)
        {
            _logger = logger;
            CreateClientAndDatabase(configuration);
        }

        private void CreateClientAndDatabase(IConfiguration configuration)
        {
            //Init CosmosDB
            _cosmosClient = new CosmosClient(configuration.GetConnectionString("CosmosDBString")); ;
            _cosmosClient.CreateDatabaseIfNotExistsAsync("ccstandarddb");
            _database = _cosmosClient.GetDatabase("ccstandarddb");
            _database.CreateContainerIfNotExistsAsync("books", "/id");
            _container = _cosmosClient.GetContainer("ccstandarddb", "books");
            // _logger.LogInformation("Container found!");

            //Init Blob Storage
            //Container created in deployment
            blobServiceClient = new BlobServiceClient((configuration.GetConnectionString("AzureStorageConnect")));
            containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            containerClient.CreateIfNotExists();

            //containerClient.UploadBlob

            //Init queue
            queue = new QueueClient(configuration.GetConnectionString("AzureStorageConnect"), bookQueueName);
            queue.CreateIfNotExists();
           

            //Init TableClient

            storageAccount = CloudStorageAccount.Parse(configuration.GetConnectionString("AzureStorageConnect"));
            tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            table = tableClient.GetTableReference(tableName);
            table.CreateIfNotExists();
        }

        // GET: api/<ValuesController>
        [HttpGet]
        public List<Book> GetAll()
        {
            List<Book> books = new List<Book>();
            foreach (Book matchingBook in _container.GetItemLinqQueryable<Book>(true))
            {
                books.Add(matchingBook);
            }

            return books;
        }

        // GET api/<ValuesController>/5
        [HttpGet("{id}")]
        public List<Book> Get(string id)
        {
            List<Book> books = new List<Book>();

            foreach (Book matchingBook in _container.GetItemLinqQueryable<Book>(true)
                       .Where(b => b.id == id))
            {
                books.Add(matchingBook);
            }
            
            return books;
        }

        // POST api/<ValuesController>
        [HttpPost("{id}")]
        public async IAsyncEnumerable<Book> Post([FromBody] Book request)
        {
            ItemResponse<Book> response = await _container.ReplaceItemAsync(
                partitionKey: new PartitionKey(request.id),
                id: request.id,
                item: request);

                Book updated = response.Resource;
            yield return updated;
        }

        // PUT api/<ValuesController>/5
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Book request)
        {
            //Web_Api.Models.Users user2 = new Web_Api.Models.Users();
            //IEnumerable<Web_Api.Models.Books> allBooks = this.GetAll();
            //return user;
            //return allBooks;

            var newBook = new Book
            {
                id = request.ISBN,
                Titel = request.Titel,
                Autor = request.Autor,
                ISBN = request.ISBN,
                Erscheinungsdatum = request.Erscheinungsdatum,
                Seitenanzahl = request.Seitenanzahl
            };


            try
            {
                var json = JsonConvert.SerializeObject(newBook);
                //var item = await _container.CreateItemAsync<Book>(newBook, new PartitionKey(newBook.ISBN));
                queue.SendMessage(json);
                //newBook.ETag = item.ETag;
            }
            catch (Exception e)
            {
                _logger.LogError($"Adding book failed with error: {e}");
                throw;
            }

            return Ok(newBook);
        }


        // DELETE api/<ValuesController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            ItemResponse<Book> response = await _container.DeleteItemAsync<Book>(
                partitionKey: new PartitionKey(id),
                id: id);

            //Book deleted = response.StatusCode;
            return Ok();
        }

        [HttpGet("/image/{id}")]
        public BlobImage ImageGet(string blobName)
        {
            //return "value";

            BlobImage returnBlob = new BlobImage();

            BlobClient blob = containerClient.GetBlobClient(blobName);
            BlobDownloadResult downloadBlob = blob.DownloadContent();

            byte [] content = downloadBlob.Content.ToArray();

            // Convert the array to a base 64 string.
            string data = Convert.ToBase64String(content);

            returnBlob.blobName = blobName;
            returnBlob.Base64Data = data;

            //Debug to console
            Console.WriteLine(data);

            //Download file if needed
            /*
            string localPath = "./data/";
            string fileName = Guid.NewGuid().ToString() + ".txt";
            string localFilePath = Path.Combine(localPath, fileName);
            string downloadFilePath = localFilePath.Replace(".jpg", "DOWNLOADED.jpg");

            Console.WriteLine("\nDownloading blob to\n\t{0}\n", downloadFilePath);

            // Download the blob's contents and save it to a file

            using (BinaryWriter writer = new BinaryWriter(System.IO.File.Open(localFilePath, FileMode.Create)))
            {
                writer.Write(downloadBlob.Content);
            }
            */

            return returnBlob;
        }

        [HttpPut("/image")]
        public void PutImage(int id, [FromBody] string value)
        {
        }

        [HttpDelete("/image/{id}")]
        public void DeleteImage(int id)
        {
        }

        // POST api/<ValuesController>
        [HttpPost("/image/{id}")]
        public void PostImage([FromBody] string value)
        {
        }

    }
}

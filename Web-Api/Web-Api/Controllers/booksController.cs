using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        public booksController(ILogger<booksController> logger, IConfiguration configuration)
        {
            _logger = logger;
            CreateClientAndDatabase(configuration);
        }

        private void CreateClientAndDatabase(IConfiguration configuration)
        {
            _cosmosClient = new CosmosClient(configuration.GetConnectionString("CosmosDBString")); ;

            _cosmosClient.CreateDatabaseIfNotExistsAsync("ccstandarddb");
            _database = _cosmosClient.GetDatabase("ccstandarddb");
            _database.CreateContainerIfNotExistsAsync("books", "/id");
            _container = _cosmosClient.GetContainer("ccstandarddb", "books");
           // _logger.LogInformation("Container found!");
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
                var item = await _container.CreateItemAsync<Book>(newBook, new PartitionKey(newBook.ISBN));
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
        public string ImageGet(int id)
        {
            return "value";
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

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Web_Api.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Queues;
using User = Web_Api.Models.User;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Web_Api.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class userController : ControllerBase
    {
        private readonly ILogger<userController> _logger;
        private CosmosClient _cosmosClient;
        private Database _database;
        private Container _container;
        string QueueName = "userinputqueue";
        QueueClient queue;

        public userController(ILogger<userController> logger, IConfiguration configuration)
        {
            _logger = logger;
            CreateClientAndDatabase(configuration);
        }

        private void CreateClientAndDatabase(IConfiguration configuration)
        {
            _cosmosClient = new CosmosClient(configuration.GetConnectionString("CosmosDBString")); ;

            _cosmosClient.CreateDatabaseIfNotExistsAsync("ccstandarddb");
            _database = _cosmosClient.GetDatabase("ccstandarddb");
            _database.CreateContainerIfNotExistsAsync("users", "/id");
            _container = _cosmosClient.GetContainer("ccstandarddb", "users");
            // _logger.LogInformation("Container found!");

            //Init queue
            queue = new QueueClient(configuration.GetConnectionString("AzureStorageConnect"), QueueName);
            queue.CreateIfNotExists();


        }

        // GET: api/<UserController>
        [HttpGet]
        public List<User> GetAll()
        {

            List<User> users = new List<User>();
            foreach (User matchingUser in _container.GetItemLinqQueryable<User>(true))
            {
                users.Add(matchingUser);
            }

            return users;
        }

        // GET api/<UserController>/5
        [HttpGet("{id}")]
        public List<User> Get(string id)
        {
            List<User> users = new List<User>();
            foreach (User matchingUser in _container.GetItemLinqQueryable<User>(true)
                .Where(b => b.id == id))
            {
                users.Add(matchingUser);
            }

            return users;
        }

        // POST api/<UserController>
        [HttpPost("{id}")]
        public async IAsyncEnumerable<User> Post([FromBody] User user)
        {
            ItemResponse<User> response = await _container.ReplaceItemAsync(
                partitionKey: new PartitionKey(user.id),
                id: user.id,
                item: user);

            User updated = response.Resource;
            yield return updated;
        }

            // PUT api/<UserController>/5
        [HttpPut]
        public IActionResult Put([FromBody] User user)

        {
            var newUser = new User
            {
                id = user.id,
                Vorname = user.Vorname,
                Nachname = user.Nachname,
                Email = user.Email,
                Hash = user.Hash
            };

            try
            {
                var json = JsonConvert.SerializeObject(newUser);
                queue.SendMessage(json);
            }
            catch (Exception e)
            {
                _logger.LogError($"Adding user failed with error: {e}");
                throw;
            }

            return Ok(newUser);
        }

        // DELETE api/<UserController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            ItemResponse<User> response = await _container.DeleteItemAsync<User>(
                partitionKey: new PartitionKey(id),
                id: id);
            return Ok();

        }
    }
}

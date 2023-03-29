using Microsoft.AspNetCore.Mvc;
using NetApi.Elastic.Models;

namespace NetApi.Elastic.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class PersonsController : ControllerBase
    {

        private static List<Person> persons = new List<Person>()
        {
            new Person{ Id=1, FullName="Usman", Email="usman@gmail.com", City="Mirpur", Country="Pakistan"},
            new Person{ Id=2, FullName="Jolly", Email="jolly@gmail.com", City="Rome", Country="Italy"},
            new Person{ Id=3, FullName="Tina", Email="tina@gmail.com", City="Berlin", Country="Germany"},
            new Person{ Id=4, FullName="Anil", Email="anil@gmail.com", City="Mumbai", Country="India"},
        };

        private readonly ILogger<PersonsController> _logger;

        public PersonsController(ILogger<PersonsController> logger)
        {
            _logger = logger;
        }

        // GET: api/<PersonsController>
        [HttpGet]
        public IEnumerable<Person> Get()
        {
            _logger.LogDebug("Getting persons");

            return persons;
        }

        // GET api/<PersonsController>/5
        [HttpGet("{id}")]
        public Person Get(int id)
        {
            _logger.LogDebug("Getting person with id {Id}", id);

            return persons.Where(x => x.Id == id).FirstOrDefault();
        }

        // POST api/<PersonsController>
        [HttpPost]
        public void Post([FromBody] Person person)
        {
            _logger.LogDebug("Adding person {@person}", person);

            persons.Add(person);

            _logger.LogInformation("Person {@person} added", person);
        }

        // PUT api/<PersonsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] Person person)
        {
            _logger.LogDebug("Updating person with id {Id}", id);

            persons.Remove(persons.Where(x => x.Id == id).FirstOrDefault());
            persons.Add(person);

            _logger.LogInformation("Person with id {Id} updated", id);
        }

        // DELETE api/<PersonsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            _logger.LogDebug("Removing person with id {Id}", id);

            persons.Remove(persons.Where(x => x.Id == id).FirstOrDefault());

            _logger.LogInformation("Person with id {Id} removed", id);
        }
    }
}

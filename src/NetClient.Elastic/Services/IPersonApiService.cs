using Refit;
using NetClient.Elastic.Models;

namespace NetClient.Elastic.Services
{
    public interface IPersonApiService
    {
        [Get("/persons")]
        Task<List<Person>> GetPersons();

        [Get("/persons/{id}")]
        Task<Person> GetPerson(int id);

        [Post("/persons")]
        Task AddPerson([Body] Person guest);

        [Put("/persons/{id}")]
        Task UpdatePerson(int id, [Body] Person guest);

        [Delete("/persons/{id}")]
        Task RemovePerson(int id);
    }
}

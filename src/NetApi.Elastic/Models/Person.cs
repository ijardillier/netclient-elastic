using Destructurama.Attributed;

namespace NetApi.Elastic.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        [NotLogged]
        public string City { get; set; }
        
        [NotLogged]
        public string Country { get; set; }
    }
}

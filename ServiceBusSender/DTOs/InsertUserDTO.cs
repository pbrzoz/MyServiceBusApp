using ServiceBusSender.Models;

namespace ServiceBusSender.DTOs
{
    public class InsertUserDTO
    {
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public int Age { get; set; }

        public User getUser(int id) {
            User user = new User { Id=id, Name=Name, LastName=LastName,Email=Email,Age=Age };
            return user;
        }
    }
}

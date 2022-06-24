namespace ServiceBusSender.DTOs
{
    public class GetUserDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }
}

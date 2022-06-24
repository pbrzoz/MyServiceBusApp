namespace ServiceBusSender.DTOs
{
    public class UpdateUserDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public int? Age { get; set; }
    }
}

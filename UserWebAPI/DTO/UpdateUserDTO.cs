namespace UserWebAPI.DTO
{
    public class UpdateUserDTO
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string HashPassword { get; set; } = null!;
        public string? Username { get; set; }
        public string? FName { get; set; }
        public string? LName { get; set; }
    }
}

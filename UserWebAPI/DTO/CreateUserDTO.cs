namespace UserWebAPI.DTO
{
    public class CreateUserDTO
    {
        public string Email { get; set; } = null!;
        public string HashPassword { get; set; } = null!;

        public string? Username { get; set; }
        public string? FName { get; set; }
        public string? LName { get; set; }
    }
}

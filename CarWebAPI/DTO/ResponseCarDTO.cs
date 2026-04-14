using CarWebAPI.Enums;

namespace CarWebAPI.DTO
{
    public class ResponseCarDTO
    {
        public string Id { get; set; } = null!;
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public int Year { get; set; }
        public decimal Price { get; set; }
        public CarColor Color { get; set; }
        public BodyType BodyType { get; set; }

        public string? ConfirmedBy { get; set; }

        public string ConfirmationStatus { get; set; } = "Pending";
    }
}

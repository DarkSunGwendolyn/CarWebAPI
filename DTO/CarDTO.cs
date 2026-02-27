using CarWebAPI.Enums;

namespace CarWebAPI.DTO
{
    public class CarDTO
    {
        public string Id { get; set; } = null!;      // для получения по ID
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public int Year { get; set; }
        public decimal Price { get; set; }
        public CarColor Color { get; set; }
        public BodyType BodyType { get; set; }
    }
}

using CarWebAPI.Enums;


namespace CarWebAPI.DTO
{
    public class CarFilterDTO
    {
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public CarColor? Color { get; set; }
        public BodyType? BodyType { get; set; }
        public string? Brand {  get; set; }
        public string? Model { get; set; }
        public int? Year { get; set; }
    }
}

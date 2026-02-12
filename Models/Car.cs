using CarWebAPI.Enums;

namespace CarWebAPI.Models
{
    public class Car
    {
        public int Id { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        public int Year { get; set; }

        public decimal Price { get; set; }

        public BodyType BodyType { get; set; }

        public CarColor Color { get; set; }

        public Car(int id, string brand, string model, 
                   int year, decimal price, BodyType bodyType, CarColor color) 
        { 
            Id = id;
            Brand = brand;
            Model = model;
            Year = year;
            Price = price;
            BodyType = bodyType;
            Color = color;
        }
    }
}

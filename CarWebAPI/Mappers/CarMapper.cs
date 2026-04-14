using CarWebAPI.DTO;
using CarWebAPI.Models;

namespace CarWebAPI.Mappers
{
    public class CarMapper : ICarMapper
    {
        public CarDTO MapToDTO(Car car)
            => new CarDTO
            {
                Id = car.Id,
                Brand = car.Brand,
                Model = car.Model,
                Year = car.Year,
                Price = car.Price,
                Color = car.Color,
                BodyType = car.BodyType,
            };

        public CreateCarDTO MapToCreateDTO(Car car)
            => new CreateCarDTO
            {
                Brand = car.Brand,
                Model = car.Model,
                Year = car.Year,
                Price = car.Price,
                Color = car.Color,
                BodyType = car.BodyType,
                ConfirmedBy = null
            };

        public UpdateCarDTO MapToUpdateDTO(Car car)
            => new UpdateCarDTO
            {
                Brand = car.Brand,
                Model = car.Model,
                Year = car.Year,
                Price = car.Price,
                Color = car.Color,
                BodyType = car.BodyType,
            };

        public ResponseCarDTO MapToResponseDTO(Car car)
            => new ResponseCarDTO
            {
                Id = car.Id,
                Brand = car.Brand,
                Model = car.Model,
                Year = car.Year,
                Price = car.Price,
                Color = car.Color,
                BodyType = car.BodyType,
                ConfirmedBy = car.ConfirmedBy,
                ConfirmationStatus = car.ConfirmationStatus,
            };


        public Car Map(CarDTO car)
            => new Car
            {
                Id = car.Id,
                Brand = car.Brand,
                Model = car.Model,
                Year = car.Year,
                Price = car.Price,
                Color = car.Color,
                BodyType = car.BodyType,
            };
        public Car Map(CreateCarDTO car)
            => new Car
            {
                Brand = car.Brand,
                Model = car.Model,
                Year = car.Year,
                Price = car.Price,
                Color = car.Color,
                BodyType = car.BodyType,
            };

        public Car Map(UpdateCarDTO car)
            => new Car
            {
                Brand = car.Brand,
                Model = car.Model,
                Year = car.Year,
                Price = car.Price,
                Color = car.Color,
                BodyType = car.BodyType,
            };
    }
}

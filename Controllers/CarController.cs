using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CarWebAPI.Models;
using CarWebAPI.Services;
using CarWebAPI.Enums;
using CarWebAPI.DTO;
using CarWebAPI.Mappers;
using System.Drawing;
using System.Text.Json.Serialization.Metadata;

namespace CarWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarController : ControllerBase
    {
        private readonly CarsService _carService;
        private ICarMapper _mapper;

        public CarController(CarsService carService) =>
            _carService = carService;

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<Car> cars = await _carService.GetAsync();

            var result = cars.Select(c => _mapper.MapToDTO(c)).ToList();
            //{
            //    Id = c.Id,
            //    Brand = c.Brand,
            //    Model = c.Model,
            //    Year = c.Year,
            //    Price = c.Price,
            //    Color = c.Color,
            //    BodyType = c.BodyType
            //}).ToList();

            return Ok(result);
        }


        //private static List<Car> Cars = new List<Car>()
        //{
        //    new Car(0, "Toyouta", "Mark", 2000, 234.67m, BodyType.HatchBack, CarColor.White),
        //    new Car(1, "Nissan", "R34", 2010, 400.89m, BodyType.HatchBack, CarColor.Blue),
        //    new Car(2, "Subaru", "Impreza", 1990, 767m, BodyType.Crossover, CarColor.Gray)
        //};

        [HttpGet("{id:length(24)}")]
        public async Task<IActionResult> Get(string id)
        {
            var car = await _carService.GetAsync(id);
            if (car is null)
                return NotFound();

            var result = _mapper.MapToDTO(car);

            //var result = new CarDTO
            //{
            //    Id = car.Id,
            //    Brand = car.Brand,
            //    Model = car.Model,
            //    Year = car.Year,
            //    Price = car.Price,
            //    Color = car.Color,
            //    BodyType = car.BodyType
            //};

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post(CreateCarDTO newCarDto)
        {
            var car = _mapper.Map(newCarDto);
            //var car = new Car
            //{
            //    Brand = newCarDto.Brand,
            //    Model = newCarDto.Model,
            //    Year = newCarDto.Year,
            //    Price = newCarDto.Price,
            //    Color = newCarDto.Color,
            //    BodyType = newCarDto.BodyType
            //};

            await _carService.CreateAsync(car);

            var result = _mapper.MapToDTO(car);

            return CreatedAtAction(nameof(Get), new { id = car.Id }, result);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, UpdateCarDTO updatedCar)
        {
            var car = await _carService.GetAsync(id);
            if (car is null)
                return NotFound();

            var updatedModel = _mapper.Map(updatedCar);
            updatedModel.Id = id;
            await _carService.UpdateAsync(id, updatedModel);

            //var result = new CarDTO
            //{
            //    Id = updatedCar.Id,
            //    Brand = updatedCar.Brand,
            //    Model = updatedCar.Model,
            //    Year = updatedCar.Year,
            //    Price = updatedCar.Price,
            //    Color = updatedCar.Color,
            //    BodyType = updatedCar.BodyType
            //};

            var result = _mapper.MapToDTO(updatedModel);
            return Ok(result);
        }

        
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var car = await _carService.GetAsync(id);
            if (car is null)
                return NotFound();

            await _carService.RemoveAsync(id);
            return NoContent();
        }

        [HttpDelete("all")]
        public async Task<IActionResult> DeleteAll()
        {
            await _carService.RemoveAllAsync();
            return NoContent();
        }
    }
}

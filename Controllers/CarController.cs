using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CarWebAPI.Models;
using CarWebAPI.Services;
using CarWebAPI.Enums;
using System.Drawing;
using System.Text.Json.Serialization.Metadata;

namespace CarWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarController : ControllerBase
    {
        private readonly CarsService _carService;

        public CarController(CarsService carService) =>
            _carService = carService;

        public async Task<IActionResult> Get()
        {
            List<Car> cars = await _carService.GetAsync();
            return Ok(cars);
        }
        //вернуть статус?


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
            return Ok(car);
        }

        [HttpPost]
        public async Task<IActionResult> Post(Car newCar)
        {
            await _carService.CreateAsync(newCar);
            return CreatedAtAction(nameof(Get), new { id = newCar.Id }, newCar);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Car updatedCar)
        {
            var car = await _carService.GetAsync(id);
            if (car is null)
                return NotFound();
            updatedCar.Id = car.Id;
            await _carService.UpdateAsync(id, updatedCar);
            return Ok(updatedCar);
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var car = await _carService.GetAsync(id);
            if (car is null)
                return NotFound();
            await _carService.RemoveAsync(id);
            return Ok(car);
        }
    }
}

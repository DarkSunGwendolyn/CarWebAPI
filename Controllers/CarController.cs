using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CarWebAPI.Models;
using CarWebAPI.Enums;
using System.Drawing;
using System.Text.Json.Serialization.Metadata;

namespace CarWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarController : ControllerBase
    {
        private static List<Car> Cars = new List<Car>()
        {
            new Car(0, "Toyouta", "Mark", 2000, 234.67m, BodyType.HatchBack, CarColor.White),
            new Car(1, "Nissan", "R34", 2010, 400.89m, BodyType.HatchBack, CarColor.Blue),
            new Car(2, "Subaru", "Impreza", 1990, 767m, BodyType.Crossover, CarColor.Gray)
        };

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(Cars);
        }

        [HttpGet("{id}")]
        public IActionResult GetById([FromRoute] int id)
        {
            var car = Cars.FirstOrDefault(c => c.Id == id);
            if (car == null)
                return NotFound();
            return Ok(car);
        }

        [HttpPost]
        public IActionResult CreateCar([FromBody] Car newCar)
        {
            Cars.Add(newCar);
            return CreatedAtAction(nameof(GetById), new { id = newCar.Id}, newCar);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateCar([FromRoute] int id, [FromBody] Car updatedCar)
        {
            var car = Cars.FirstOrDefault(c => c.Id == id);
            car.Brand = updatedCar.Brand;
            car.Model = updatedCar.Model;
            car.Year = updatedCar.Year;
            car.Price = updatedCar.Price;
            car.BodyType = updatedCar.BodyType;
            car.Color = updatedCar.Color;
            return Ok(car);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteCar([FromRoute] int id)
        {
            var car = Cars.FirstOrDefault(c => c.Id == id);
            Cars.Remove(car);
            return NoContent();
        }
    }
}

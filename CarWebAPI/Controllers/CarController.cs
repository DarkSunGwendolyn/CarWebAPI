using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CarWebAPI.Models;
using CarWebAPI.Services;
using CarWebAPI.Enums;
using CarWebAPI.DTO;
using CarWebAPI.Mappers;
using System.Drawing;
using System.Text.Json.Serialization.Metadata;
using CarWebAPI.Telemetry;

namespace CarWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarController : ControllerBase
    {
        private readonly CarsService _carService;
        private readonly KafkaRequestProducer _kafkaProducer;
        private ICarMapper _mapper;

        public CarController(CarsService carService, ICarMapper mapper, KafkaRequestProducer kafkaProducer)
        {
            _carService = carService;
            _mapper = mapper;
            _kafkaProducer = kafkaProducer;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<Car> cars = await _carService.GetAsync();
            var result = cars.Select(c => _mapper.MapToResponseDTO(c)).ToList();
            return Ok(result);
        }

        [HttpGet("{id:length(24)}")]
        public async Task<IActionResult> Get(string id)
        {
            var car = await _carService.GetAsync(id);
            if (car is null)
                return NotFound();
            var result = _mapper.MapToResponseDTO(car);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post(CreateCarDTO newCarDto)
        {
            var car = _mapper.Map(newCarDto);
            car.ConfirmedAt = null;
            car.ConfirmationStatus = "Pending";
            await _carService.CreateAsync(car);
            //await _kafkaProducer.SendRequestAsync(car.Id, newCarDto.ConfirmedBy);
            _ = Task.Run(() => _kafkaProducer.SendRequestAsync(car.Id, newCarDto.ConfirmedBy));
            var result = _mapper.MapToResponseDTO(car);
            result.ConfirmationStatus = "Pending";
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
            updatedModel.ConfirmationStatus = car.ConfirmationStatus;
            await _carService.UpdateAsync(id, updatedModel);
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

        [HttpGet("filter")]
        public async Task<IActionResult> Filter(
            [FromQuery] int? minPrice,
            [FromQuery] int? maxPrice,
            [FromQuery] string? brand,
            [FromQuery] string? model,
            [FromQuery] CarColor? color,
            [FromQuery] BodyType? bodyType,
            [FromQuery] int? year
            )
        {
            var filterDTO = new CarFilterDTO
            {
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Brand = brand,
                Model = model,
                Color = color,
                BodyType = bodyType,
                Year = year
            };

            var cars = await _carService.FilterAsync(filterDTO);
            var result = cars.Select(c => _mapper.MapToDTO(c)).ToList();
            return Ok(result);
        }

    }
}

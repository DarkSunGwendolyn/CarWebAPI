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
        private ICarMapper _mapper;

        public CarController(CarsService carService, ICarMapper mapper)
        {
            _carService = carService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<Car> cars = await _carService.GetAsync();
            var result = cars.Select(c => _mapper.MapToDTO(c)).ToList();
            return Ok(result);
        }

        [HttpGet("{id:length(24)}")]
        public async Task<IActionResult> Get(string id)
        {
            var car = await _carService.GetAsync(id);
            if (car is null)
                return NotFound();
            var result = _mapper.MapToDTO(car);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post(CreateCarDTO newCarDto)
        {
            var car = _mapper.Map(newCarDto);
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
        public async Task<IActionResult> Filter(CarFilterDTO filterDTO)
        {
            var cars = await _carService.FilterAsync(filterDTO);
            var result = cars.Select(c => _mapper.MapToDTO(c)).ToList();
            return Ok(result);
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserWebAPI.Models;
using UserWebAPI.DTO;
using UserWebAPI.Mappers;
using UserWebAPI.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace UserWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UsersService _usersService;
        private IUserMapper _mapper;

        public UserController(UsersService usersService, IUserMapper mapper)
        {
            _usersService = usersService;
            _mapper = mapper;   
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<User> users = await _usersService.GetAsync();
            var results = users.Select(c => _mapper.MapToDTO(c)).ToList();
            return Ok(results);
        }

        [HttpGet]
        public async Task<IActionResult> Get(string id)
        {
            var user = await _usersService.GetAsync(id);
            if (user is null)
                return NotFound();
            var result = _mapper.MapToDTO(user);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post(CreateUserDTO userDTO)
        {
            var user = _mapper.Map(userDTO);
            await _usersService.CreateAsync(user);
            var result = _mapper.MapToDTO(user);
            return Ok(result);
        }

        [HttpPut("{id:length(24}")]
        public async Task<IActionResult> Update(string id, UpdateUserDTO updatedUser)
        {
            var user = await _usersService.GetAsync(id);
            if (user is null)
                return NotFound();
            var updatedModel = _mapper.Map(updatedUser);
            updatedModel.Id = id;
            await _usersService.UpdateAsync(id, updatedModel);
            var result = _mapper.MapToDTO(updatedModel);
            return Ok(result);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _usersService.GetAsync(id);
            if (user is null)
                return NotFound();
            await _usersService.RemoveAsync(id);
            return NoContent();
        }
        [HttpDelete("all")]
        public async Task<IActionResult> DeleteAll()
        {
            await _usersService.RemoveAllAsync();
            return NoContent();
        }



    }
}

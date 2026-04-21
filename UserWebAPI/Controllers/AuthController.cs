using Microsoft.AspNetCore.Mvc;
using UserWebAPI.Services;
using UserWebAPI.DTO;

namespace UserWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _auth;
        private readonly UsersService _usersService;

        public AuthController(AuthService auth, UsersService usersService)
        {
            _auth = auth;
            _usersService = usersService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserDTO loginUser)
        {
            var user = await _usersService.GetByEmailAsync(loginUser.Email);

            if (user == null)
            {
                return Unauthorized("User not found");
            }

            if(user.HashPassword != loginUser.Password)
            {
                return Unauthorized("Invalid password");
            }

            var token = _auth.GenerateToken(user.Id);
            return Ok(new {token});
        }
    }
}

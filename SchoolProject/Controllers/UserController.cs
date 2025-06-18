using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.DTOs.UserDtos;
using SchoolProject.Repository;

namespace SchoolProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserRepository _userRepository;

        public UserController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] UserRegisterDTos registerDto)
        {
            var (success, message) = await _userRepository.RegisterAsync(registerDto);
            if (success)
                return Ok(new { message });

            return BadRequest(new { error = message });
        }
      
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLogInDTos loginDto)
        {
            var (success, tokenOrMessage) = await _userRepository.LoginAsync(loginDto);
            if (success)
                return Ok(new { token = tokenOrMessage });

            return Unauthorized(new { error = tokenOrMessage });
        }

        //[AllowAnonymous]
        //[HttpPost("assign-role")]
        //public async Task<IActionResult> AssignRole([FromBody] UserRoleAssignDto dto)
        //{
        //    var (success, message) = await _userRepository.AssignRoleToUserAsync(dto);
        //    if (success)
        //        return Ok(message);
        //    else
        //        return BadRequest(message);
        //}

    }
}

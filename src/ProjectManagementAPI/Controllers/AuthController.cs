using Microsoft.AspNetCore.Mvc;
using ProjectManagementAPI.DTOs;
using ProjectManagementAPI.Models;
using ProjectManagementAPI.Services;
using System.IdentityModel.Tokens.Jwt;


namespace ProjectManagementAPI.Controllers
{

    [Route("api/")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;

        public AuthController(UserService userService)
        {

            _userService = userService;
        }

        [HttpPost("signup")]
        public async Task<ActionResult> Register(RegisterDTO dto)
        {
            ActionResult result;
            User user;

            if(await _userService.VerifyEmailExistsAsync(dto.Email))
            {
                result = BadRequest("Email already in use.");
            }
            else
            {
                user = await _userService.CreateUserAsync(dto);
                result = Ok(user);
            }

            return result;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDTO dto)
        {
            ActionResult result = BadRequest("Email and password are required.");

            if (dto.Email != null && dto.Password != null)
            {
                string role = "";
                User? user = await _userService.Login(dto.Email, dto.Password);

                if(user == null)
                {
                    result = Unauthorized();
                    
                }
                else if(user.GetType() == typeof(Teacher))
                {
                    role = "Teacher";
                }
                else
                {
                    role = "Student";
                }

                if(role != "")
                {
                    JwtSecurityToken token = _userService.GenerateJwtToken(user, role);
                    result = Ok(new{token = new JwtSecurityTokenHandler().WriteToken(token)});
                }
            }

            return result;
        }
    }
}
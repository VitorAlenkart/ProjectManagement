using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.DependencyResolver;
using ProjectManagementAPI.Data;
using ProjectManagementAPI.DTOs;
using ProjectManagementAPI.Models;
using ProjectManagementAPI.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;


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
        public async Task<ActionResult> Register([FromBody] JsonElement json)
        {
            String? email = json.TryGetProperty("email", out var emailProperty) ? emailProperty.GetString() : null;
            String? fullName = json.TryGetProperty("fullName", out var fullNameProperty) ? fullNameProperty.GetString() : null;
            String? occupationArea = json.TryGetProperty("occupationArea", out var occupationAreaProperty) ? occupationAreaProperty.GetString() : null;
            String? formationArea = json.TryGetProperty("formationArea", out var formationAreaProperty) ? formationAreaProperty.GetString() : null;
            String? educationalInstitution = json.TryGetProperty("educationalInstitution", out var educationalInstitutionProperty) ? educationalInstitutionProperty.GetString() : null;
            String? password = json.TryGetProperty("password", out var passwordProperty) ? (passwordProperty.GetString()!) : null;

            ActionResult result;
            
            if (IsValidUserType(json))
            {
                if(_userService.verifyEmailExists(email)) {
                    result = BadRequest("Email already in use.");
                }
                else
                {
                    User user = _userService.createUser(fullName, email, password, educationalInstitution, occupationArea, formationArea);
                    result = Ok(user);
                }
            }
            else
            {
                result = BadRequest("Invalid registration data.");
            }
        
            return result;
        }

        private bool IsValidUserType(JsonElement json)
        {
            string? email = json.TryGetProperty("email", out var emailProperty) ? emailProperty.GetString() : null;
            string? fullName = json.TryGetProperty("fullName", out var fullNameProperty) ? fullNameProperty.GetString() : null;
            string? password = json.TryGetProperty("password", out var passwordProperty) ? (passwordProperty.GetString()!) : null;  
            string? occupationArea = json.TryGetProperty("occupationArea", out var occupationAreaProperty) ? occupationAreaProperty.GetString() : null;
            string? formationArea = json.TryGetProperty("formationArea", out var formationAreaProperty) ? formationAreaProperty.GetString() : null;
            string? educationalInstitution = json.TryGetProperty("educationalInstitution", out var educationalInstitutionProperty) ? educationalInstitutionProperty.GetString() : null;
            bool result = false;
            if((email != null && email.Contains("@")))
            {
                if (fullName != null && fullName.Length > 2)
                {
                    if (password != null && password.Length >= 6)
                    {
                        if(occupationArea != null && occupationArea.Length > 2 && formationArea != null && formationArea.Length > 2)
                        {
                            result = true;
                        }
                        else if (educationalInstitution != null && educationalInstitution.Length > 2)
                        {
                            result = true;
                        }
                    }
                }
            } 
            return result;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDto dto)
        {
            ActionResult result = BadRequest("Email and password are required.");

            if (dto.Email != null && dto.Password != null)
            {
                string role = "";
                User? user = _userService.login(dto.Email, dto.Password);

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
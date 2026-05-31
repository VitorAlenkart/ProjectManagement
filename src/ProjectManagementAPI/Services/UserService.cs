using Microsoft.IdentityModel.Tokens;
using ProjectManagementAPI.DTOs;
using ProjectManagementAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ProjectManagementAPI.Repositories.Interfaces;

namespace ProjectManagementAPI.Services
{
    public class UserService(IUserRepository repository, PasswordService passwordService, IConfiguration configuration)
    {
        private readonly IUserRepository _repository = repository;
        private readonly PasswordService _passwordService = passwordService;
        private readonly IConfiguration _configuration = configuration;

        public async Task<Student> CreateStudentAsync(RegisterDTO studentDto)
        {
            Student student = new()
            {
                FullName = studentDto.FullName,
                Email = studentDto.Email,
                HashedPassword = _passwordService.HashPassword(studentDto.Email, studentDto.Password),
                EducationalInstitution = studentDto.EducationalInstitution!
            };

            await _repository.AddStudentAsync(student);
            await _repository.SaveChangesAsync();

            return student;

        }

        public async Task<Teacher> CreateTeacherAsync(RegisterDTO teacherDto)
        {
            Teacher teacher = new()
            {
                FullName = teacherDto.FullName,
                Email = teacherDto.Email,
                HashedPassword = _passwordService.HashPassword(teacherDto.Email, teacherDto.Password),
                OccupationArea = teacherDto.OccupationArea!,
                FormationArea = teacherDto.FormationArea!
            };

            await _repository.AddTeacherAsync(teacher);

            await _repository.SaveChangesAsync();

            return teacher;
        }

        public async Task<User> CreateUserAsync(RegisterDTO dto)
        {
            User user;
            if (dto.UserType == "Teacher")
            {
                user = await CreateStudentAsync(dto);
            }
            else if (dto.UserType == "User")
            {
                user = await CreateTeacherAsync(dto);
            }
            else
            {
                throw new ArgumentException("Invalid user type");
            }

            return user;
        }

        public async Task<bool> VerifyEmailExistsAsync(string email)
        {
            bool result =  await _repository.EmailExistsAsync(email);

            return result;
        }

        public async Task<User?> Login(string email, string password)
        {
            User? user = await _repository.GetUserByEmailAsync(email);

            if (user != null)
            {
                if (!(_passwordService.VerifyPassword(email, password)))
                {
                    user = null;
                }
            }

            return user;
        }

        public JwtSecurityToken GenerateJwtToken(User user, string role)
        {
            var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, role)
                    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return token;
        }

        public async Task<bool> UserExists(int id)
        {
            bool exists = await _repository.UserExistsAsync(id);

            return exists;
        }

        public async Task<Student?> GetStudentById(int id)
        {
            Student? result = await _repository.GetStudentByIdAsync(id);

            return result;
        }
    }
}


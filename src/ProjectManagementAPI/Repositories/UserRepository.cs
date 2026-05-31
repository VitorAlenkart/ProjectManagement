using Microsoft.EntityFrameworkCore;
using ProjectManagementAPI.Data;
using ProjectManagementAPI.Models;
using ProjectManagementAPI.Repositories.Interfaces;

namespace ProjectManagementAPI.Repositories
{
    public class UserRepository(ApplicationContext context) : IUserRepository
    {
        private readonly ApplicationContext _context = context;

        public async Task AddStudentAsync(Student student)
        {
            await _context.Students.AddAsync(student);
        }

        public async Task AddTeacherAsync(Teacher teacher)
        {
            await _context.Teachers.AddAsync(teacher);
        }

        public async Task<Student?> GetStudentByIdAsync(int id)
        {
            Student? student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id);


            return student;
        }

        public async Task<Student?> GetStudentByEmailAsync(string email)
        {
            Student? student = await _context.Students
                .FirstOrDefaultAsync(s => s.Email == email);

            return student;
        }

        public async Task<Teacher?> GetTeacherByIdAsync(int id)
        {
            Teacher? teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == id);

            return teacher;
        }

        public async Task<Teacher?> GetTeacherByEmailAsync(string email)
        {
            Teacher? teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Email == email);

            return teacher;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            User? user = (User?) await _context.Students
                .FirstOrDefaultAsync(s => s.Id == id) ??
                await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == id);
            
            return user;
        }

        public async Task<User?> GetUserEmailAsync(string email)
        {
            User? user = (User?)await _context.Students
                .FirstOrDefaultAsync(s => s.Email == email) ??
                await _context.Teachers
                .FirstOrDefaultAsync(t => t.Email == email);

            return user;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            bool exists = await _context.Students
                .AnyAsync(s => s.Email == email) ||
                await _context.Teachers
                .AnyAsync(t => t.Email == email);

            return exists;
        }

        public async Task<bool> UserExistsAsync(int id)
        {
            bool exists = await _context.Students
                .AnyAsync(s => s.Id == id) 
                || await _context.Teachers
                .AnyAsync(t => t.Id == id);

            return exists;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

    }

}

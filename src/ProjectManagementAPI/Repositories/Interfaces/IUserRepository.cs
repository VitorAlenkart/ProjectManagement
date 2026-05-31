using ProjectManagementAPI.Models;

namespace ProjectManagementAPI.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task AddStudentAsync(Student student);

        Task AddTeacherAsync(Teacher teacher);

        Task<Student?> GetStudentByIdAsync(int id);

        Task<Student?> GetStudentByEmailAsync(string email);

        Task<Teacher?> GetTeacherByIdAsync(int id);

        Task<Teacher?> GetTeacherByEmailAsync(string email);

        Task<User?> GetUserByIdAsync(int id);

        Task<User?> GetUserByEmailAsync(string email);

        Task<bool> EmailExistsAsync(string email);

        Task<bool> UserExistsAsync(int id);

        Task SaveChangesAsync();
    }
}

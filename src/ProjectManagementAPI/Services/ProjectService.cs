using Microsoft.EntityFrameworkCore;
using ProjectManagementAPI.Data;
using ProjectManagementAPI.DTOs;
using ProjectManagementAPI.Models;
using System.Text.Json;

namespace ProjectManagementAPI.Services
{
    public class ProjectService
    {
        private readonly ApplicationContext _context;
        private readonly UserService _userService;
        private readonly ILogger<ProjectService> _logger;

        public ProjectService(ApplicationContext context, UserService userService, ILogger<ProjectService> logger)
        {
            _context = context;
            _userService = userService;
            _logger = logger;
        }

        public async Task<List<Project>> GetAllProjects()
        {
            List<Project> result = await _context.Projects.ToListAsync();

            return result;
        }

        public async Task<DetailedProjectDTO?> GetDetailedProject(int id)
        {
            DetailedProjectDTO? result = null;
            var project = await _context.Projects.FindAsync(id);

            if (project != null)
            {
                var relations = await _context.StudentProjects
                    .Where(sp => sp.ProjectId == id)
                    .ToListAsync();

                var students = new List<StudentDTO>();

                foreach (var relation in relations)
                {
                    var student = await _userService.GetStudentById(relation.StudentId);

                    if (student != null)
                    {
                        students.Add(new StudentDTO
                        {
                            Id = student.Id,
                            FullName = student.FullName,
                            Email = student.Email,
                            EducationalInstitution = student.EducationalInstitution,
                            Role = relation.Role
                        });
                    }
                }

                result = new DetailedProjectDTO
                {
                    id = project.Id,
                    name = project.Name,
                    description = project.Description,
                    date = project.Date,
                    teacherId = project.TeacherId,
                    students = students
                };
            }

            return result;
        }

        public async Task<Project?> CreateProject(JsonElement json, int teacherId)
        {
            Project? result;
            string? name = json.TryGetProperty("name", out var nameProperty) ? nameProperty.GetString() : null;
            string? description = json.TryGetProperty("description", out var descriptionProperty) ? descriptionProperty.GetString() : null;

            if(name == null || description == null)
            {
                result = null;
            }
            else
            {
                Project project = new()
                {
                    Name = name,
                    Description = description,
                    TeacherId = teacherId,
                    Date = DateTime.Now
                };
                _context.Projects.Add(project);
                await _context.SaveChangesAsync();
                result = project;
                _logger.LogInformation("Project {Title} has added for teacher {teacherId}", project.Name, teacherId);
            }
            return result;
        }

        public async Task<Project?> UpdateProject(int projectId, string name, string description, int teacherId)
        {
            Project? result = null;
            Project? project = await GetProjectById(projectId);
            if (project == null)
            {
                _logger.LogInformation("Teacher {teacherId} tryed update project {projectId}, but project don't", teacherId, projectId);

            }
            else if(ProjectBelongsToTeacher(projectId, teacherId))
            {
                project.Name = name;
                project.Description = description;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Teacher {teacherId} update project {projectId}", teacherId, projectId);

            }else if(!ProjectBelongsToTeacher(projectId, teacherId))
            {
                _logger.LogInformation("Teacher {teacherId} tryed update project {projectId} without permission", teacherId, projectId);
            }
            
            return result;
        }

        public async Task<Project?> DeleteProject(int projectId, int teacherId)
        {
            Project? project = await GetProjectById(projectId);
            Project? result; 
            if (project == null)
            {
                result = null;
            }else if (!ProjectBelongsToTeacher(projectId, teacherId))
            {
                _logger.LogInformation("Teacher {teacherId} tried delete project {projectId}, " +
                   "but don't have permission", teacherId, projectId);
                result = null;
            }
            else
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
                result = project;
            }

            return result;
        }

        public async Task<StudentProject?> AddStudentToProject(int projectId, int studentId, string role,int teacherId)
        {
            StudentProject? result = null;
            if (!ProjectBelongsToTeacher(projectId,teacherId))
            {
                _logger.LogInformation("Teacher {teacherId} tried to put student {studentId} in the project {projectId}, but teacher don't hava permission", teacherId, studentId, projectId);
                result = null;
            }
            else
            {
                StudentProject relation = new()
                {
                    ProjectId = projectId,
                    StudentId = studentId,
                    Role = role
                };

                _context.StudentProjects.Add(relation);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Teacher {teacherId} put student {studentId} in the project {projectId}", teacherId, studentId, projectId);
                result = relation;
            }

            return result;
        }

        public async Task<StudentProject?> DeleteStudentFromProject(int projectId, int studentId, int teacherId)
        {
            StudentProject? relation = await _context.StudentProjects
                .FirstOrDefaultAsync(sp => sp.ProjectId == projectId && sp.StudentId == studentId);
            StudentProject? result = null;
            if (!ProjectBelongsToTeacher(projectId,teacherId))
            {
                _logger.LogInformation("Teacher {teacherId} tried to delete student {studentId} to the project {projectId}, but teacher don't hava permission", teacherId, studentId, projectId);
                result = null;
            }
            else if(relation != null)
            {
                _context.StudentProjects.Remove(relation);
                await _context.SaveChangesAsync();
                result = relation;
                _logger.LogInformation("Teacher {teacherId} deleted student {studentId} from the project {projectId}", teacherId, studentId, projectId);
            }

            return result;
        }

        public bool ProjectExists(int id)
        {
            bool result = _context.Projects.Any(e => e.Id == id);

            return result;
        }

        public bool TeacherExists(int id)
        {
            bool result = _context.Teachers.Any(e => e.Id == id);

            return result;
        }

        public bool ProjectBelongsToTeacher(int projectId, int teacherId)
        {
            bool result = _context.Projects.Any(p => p.Id == projectId && p.TeacherId == teacherId);

            return result;
        }

        public async Task<Project?> GetProjectById(int id)
        {
            Project? result = await _context.Projects.FindAsync(id);

            return result;
        }

        public bool StudentBelongsToProject(int projectId, int studentId)
        {
            bool result = _context.StudentProjects.Any(sp => sp.ProjectId == projectId && sp.StudentId == studentId);

            return result;
        }
    }
}

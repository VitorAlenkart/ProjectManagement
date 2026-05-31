using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementAPI.DTOs;
using ProjectManagementAPI.Models;
using ProjectManagementAPI.Services;
using System.Security.Claims;
using System.Text.Json;

namespace ProjectManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly ProjectService _projectService;
        private readonly UserService _userService;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(ProjectService projectService, UserService userService, ILogger<ProjectsController> logger)
        {
            _projectService = projectService;
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
        {
            var projects = await _projectService.GetAllProjects();
            var result = Ok(projects);

            return result;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DetailedProjectDTO>> GetProject(int id)
        {
            ActionResult<DetailedProjectDTO> result;
            var project = await _projectService.GetDetailedProject(id);

            if (project == null)
            {
                result = NotFound();
            }
            else
            {
                result = Ok(project);
            }

            return result;
        }

        [Authorize(Roles = "Teacher")]
        [HttpPut("{id}")]
        public async Task<ActionResult<Project>> PutProject(int id, UpdateProjectDto dto)
        {
            ActionResult result;
            int teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (_projectService.ProjectExists(id))
            {
                var project = _projectService.UpdateProject(id, dto.Name, dto.Description, teacherId);
                if(project == null)
                {
                    result = Unauthorized("Teacher can't delete this project");
                }
                else
                {
                    result = Ok(project);
                }
            }
            else
            {
                result = NotFound("Project not found");
            }

            return result;
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        public async Task<ActionResult<Project>> PostProject([FromBody] JsonElement json)
        {
            ActionResult<Project> result;
            int teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            Project? project = await _projectService.CreateProject(json, teacherId);
            
            if (project == null)
            {
                result = BadRequest("Name and description are required.");
            }
            else
            {   
                result = Ok(project);
            }
            return result;
        }

        [Authorize(Roles = "Teacher")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProject(int id)
        {
            ActionResult result;
            int teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (!_projectService.ProjectExists(id))
            {
                result = NotFound();
                _logger.LogInformation("Teacher {teacherId} tried delete project {projectId}, " +
                                       "but project don't exists", teacherId, id);
            }
            else
            {
                var project = await _projectService.DeleteProject(id, teacherId);
                if (project == null)
                {
                    result = Forbid("Teacher can't delete this project");
                }
                else
                {
                    result = Ok(project);
                }
            }
            return result;
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost("link/{projectId}/students")]
        public async Task<ActionResult> AddStudentToProject(int projectId, StudentProjectDTO dto)
        {
            ActionResult result;
            int teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (!_projectService.ProjectExists(projectId))
            {
                result = NotFound("Project not found");
            }
            else if (!await _userService.UserExists(dto.StudentId))
            {
                result = NotFound("Student not found");
                _logger.LogInformation("Teacher {teacherId} tried add a student to project {projectId}, but student don't exists", teacherId, projectId);
            }
            else if (_projectService.StudentBelongsToProject(projectId, dto.StudentId))
            {
                result = BadRequest("Student already in Project");
                _logger.LogInformation("Teacher {teacherId} tried add a student to project {projectId}, but Student already in Project", teacherId, projectId);
            }
            else
            {
                var relation = await _projectService.AddStudentToProject(projectId, dto.StudentId, dto.Role, teacherId);
                if(relation == null)
                {
                    result = Unauthorized("Teacher can't add students in this project");
                }
                else
                {
                    result = Ok(relation);
                }
            }

            return result;
        }

        [Authorize(Roles = "Teacher")]
        [HttpDelete("{projectId}/unlink/{studentId}")]
        public async Task<ActionResult> DeleteStudentFromProject(int projectId, int studentId)
        {
            ActionResult result;
            int teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (!_projectService.ProjectExists(projectId))
            {
                result = NotFound("Project not found");
            }
            else if (! await _userService.UserExists(studentId))
            {
                result = NotFound("Student not found");
                _logger.LogInformation("Teacher {teacherId} tried delete a student from project {projectId}, but student don't exists", teacherId, projectId);
            }
            else if (!_projectService.StudentBelongsToProject(projectId, studentId))
            {
                result = NotFound("Student not in project.");
                _logger.LogInformation("Teacher {teacherId} tried delete a student from project {projectId}, but student don't belongs to the project", teacherId, projectId);
            }
            else
            {
                var relation = await _projectService.DeleteStudentFromProject(projectId, studentId, teacherId);
                if (relation == null)
                {
                    result = Unauthorized("Teacher can't delete students from this project");
                }
                else
                {
                    result = Ok(relation);
                }
            }

            return result;
        }
    }
}

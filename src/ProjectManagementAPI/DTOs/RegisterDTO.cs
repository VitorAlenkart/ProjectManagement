using System.ComponentModel.DataAnnotations;

namespace ProjectManagementAPI.DTOs
{
    public class RegisterDTO
    {
        [Required]
        public string UserType { get; set; } = null!;

        [Required]
        [MinLength(3)]
        public string FullName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = null!;

        public string?
            EducationalInstitution
        { get; set; }

        public string?
            OccupationArea
        { get; set; }

        public string?
            FormationArea
        { get; set; }
    }
}

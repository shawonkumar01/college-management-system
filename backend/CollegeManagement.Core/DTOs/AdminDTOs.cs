using CollegeManagement.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace CollegeManagement.Core.DTOs
{
    // User DTOs
    public class CreateUserRequest
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }
    }

    public class UpdateUserRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public UserRole? Role { get; set; }
    }

    // Class DTOs
    public class CreateClassRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Code { get; set; }
        public string? Description { get; set; }
        public int? TeacherId { get; set; }
    }

    public class UpdateClassRequest
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int? TeacherId { get; set; }
    }

    public class ClassDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int? TeacherId { get; set; }
        public string? TeacherName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Subject DTOs
    public class CreateSubjectRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Code { get; set; }
        public string? Description { get; set; }
        public int? TeacherId { get; set; }
    }

    public class UpdateSubjectRequest
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int? TeacherId { get; set; }
    }

    public class SubjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int? TeacherId { get; set; }
        public string? TeacherName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Enrollment DTOs
    public class CreateEnrollmentRequest
    {
        [Required]
        public int ClassId { get; set; }

        [Required]
        public int StudentId { get; set; }
    }
}

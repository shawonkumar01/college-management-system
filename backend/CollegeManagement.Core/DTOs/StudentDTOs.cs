using CollegeManagement.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace CollegeManagement.Core.DTOs
{
    public class CreateSubmissionRequest
    {
        [Required]
        [MaxLength(5000)]
        public string Answer { get; set; } = string.Empty;
    }

    public class UpdateSubmissionRequest
    {
        [Required]
        [MaxLength(5000)]
        public string Answer { get; set; } = string.Empty;
    }

    public class StudentAssignmentDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime Deadline { get; set; }
        public int MaxMarks { get; set; }
        public bool AllowLateSubmission { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public bool HasSubmitted { get; set; }
        public SubmissionDto? Submission { get; set; }
    }

    public class StudentSubmissionDto
    {
        public int Id { get; set; }
        public string Answer { get; set; } = string.Empty;
        public SubmissionStatus Status { get; set; }
        public int? MarksObtained { get; set; }
        public string? Feedback { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public int AssignmentId { get; set; }
        public string AssignmentTitle { get; set; } = string.Empty;
        public DateTime AssignmentDeadline { get; set; }
        public int AssignmentMaxMarks { get; set; }
        public bool CanUpdate { get; set; }
    }
}

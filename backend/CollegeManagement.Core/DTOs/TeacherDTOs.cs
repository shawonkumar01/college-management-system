using CollegeManagement.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace CollegeManagement.Core.DTOs
{
    // Assignment DTOs
    public class CreateAssignmentRequest
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime Deadline { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int MaxMarks { get; set; }

        public AssignmentStatus Status { get; set; } = AssignmentStatus.Draft;

        public bool AllowLateSubmission { get; set; } = false;

        [Required]
        public int ClassId { get; set; }

        [Required]
        public int SubjectId { get; set; }
    }

    public class UpdateAssignmentRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? Deadline { get; set; }
        public int? MaxMarks { get; set; }
        public AssignmentStatus? Status { get; set; }
        public bool? AllowLateSubmission { get; set; }
    }

    public class AssignmentDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime Deadline { get; set; }
        public int MaxMarks { get; set; }
        public AssignmentStatus Status { get; set; }
        public bool AllowLateSubmission { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
    }

    // Submission DTOs
    public class GradeSubmissionRequest
    {
        [Required]
        public int MarksObtained { get; set; }

        public string? Feedback { get; set; }

        public SubmissionStatus Status { get; set; } = SubmissionStatus.Reviewed;
    }

    public class SubmissionDto
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
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
    }

    public class UpdateSubmissionStatusRequest
    {
        [Required]
        public SubmissionStatus Status { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollegeManagement.Core.Entities
{
    public enum SubmissionStatus
    {
        Submitted = 1,
        Reviewed = 2,
        Returned = 3
    }

    public class Submission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(5000)]
        public string Answer { get; set; } = string.Empty;

        public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;

        public int? MarksObtained { get; set; }

        [MaxLength(2000)]
        public string? Feedback { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }

        // Foreign keys
        [Required]
        public int AssignmentId { get; set; }

        [Required]
        public int StudentId { get; set; }

        // Navigation properties
        [ForeignKey("AssignmentId")]
        public virtual Assignment Assignment { get; set; } = null!;

        [ForeignKey("StudentId")]
        public virtual User Student { get; set; } = null!;
    }
}

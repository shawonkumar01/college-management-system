using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollegeManagement.Core.Entities
{
    public class Enrollment
    {
        [Key]
        public int Id { get; set; }

        // Foreign keys
        [Required]
        public int ClassId { get; set; }

        [Required]
        public int StudentId { get; set; }

        public DateTime EnrolledAt { get; set; } = System.DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; } = null!;

        [ForeignKey("StudentId")]
        public virtual User Student { get; set; } = null!;
    }
}

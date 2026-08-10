using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollegeManagement.Core.Entities
{
    public class Subject
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Code { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = System.DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Foreign key
        public int? TeacherId { get; set; }

        // Navigation properties
        [ForeignKey("TeacherId")]
        public virtual User? Teacher { get; set; }

        public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}

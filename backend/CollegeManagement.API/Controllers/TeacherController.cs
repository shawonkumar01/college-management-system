using CollegeManagement.Core.DTOs;
using CollegeManagement.Core.Entities;
using CollegeManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CollegeManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TeacherController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TeacherController> _logger;

        public TeacherController(AppDbContext context, ILogger<TeacherController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private UserRole GetCurrentUserRole()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : UserRole.Student;
        }

        private bool IsTeacher()
        {
            return GetCurrentUserRole() == UserRole.Teacher;
        }

        // Assignment Management
        [HttpGet("assignments")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetMyAssignments()
        {
            try
            {
                var teacherId = GetCurrentUserId();
                var assignments = await _context.Assignments
                    .Include(a => a.Class)
                    .Include(a => a.Subject)
                    .Where(a => a.Subject.TeacherId == teacherId)
                    .Select(a => new AssignmentDto
                    {
                        Id = a.Id,
                        Title = a.Title,
                        Description = a.Description,
                        Deadline = a.Deadline,
                        MaxMarks = a.MaxMarks,
                        Status = a.Status,
                        AllowLateSubmission = a.AllowLateSubmission,
                        CreatedAt = a.CreatedAt,
                        ClassId = a.ClassId,
                        ClassName = a.Class.Name,
                        SubjectId = a.SubjectId,
                        SubjectName = a.Subject.Name
                    })
                    .ToListAsync();

                return Ok(assignments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting teacher assignments");
                return StatusCode(500, new { message = "An error occurred while retrieving assignments" });
            }
        }

        [HttpGet("assignments/{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetAssignmentById(int id)
        {
            try
            {
                var teacherId = GetCurrentUserId();
                var assignment = await _context.Assignments
                    .Include(a => a.Class)
                    .Include(a => a.Subject)
                    .FirstOrDefaultAsync(a => a.Id == id && a.Subject.TeacherId == teacherId);

                if (assignment == null)
                {
                    return NotFound(new { message = "Assignment not found" });
                }

                var assignmentDto = new AssignmentDto
                {
                    Id = assignment.Id,
                    Title = assignment.Title,
                    Description = assignment.Description,
                    Deadline = assignment.Deadline,
                    MaxMarks = assignment.MaxMarks,
                    Status = assignment.Status,
                    AllowLateSubmission = assignment.AllowLateSubmission,
                    CreatedAt = assignment.CreatedAt,
                    ClassId = assignment.ClassId,
                    ClassName = assignment.Class.Name,
                    SubjectId = assignment.SubjectId,
                    SubjectName = assignment.Subject.Name
                };

                return Ok(assignmentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assignment");
                return StatusCode(500, new { message = "An error occurred while retrieving assignment" });
            }
        }

        [HttpPost("assignments")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CreateAssignment([FromBody] CreateAssignmentRequest request)
        {
            try
            {
                var teacherId = GetCurrentUserId();

                // Verify teacher owns the subject
                var subject = await _context.Subjects.FindAsync(request.SubjectId);
                if (subject == null || subject.TeacherId != teacherId)
                {
                    return BadRequest(new { message = "Invalid subject or you don't have permission" });
                }

                // Verify class exists
                var classEntity = await _context.Classes.FindAsync(request.ClassId);
                if (classEntity == null)
                {
                    return BadRequest(new { message = "Invalid class" });
                }

                var assignment = new Assignment
                {
                    Title = request.Title,
                    Description = request.Description,
                    Deadline = request.Deadline,
                    MaxMarks = request.MaxMarks,
                    Status = request.Status,
                    AllowLateSubmission = request.AllowLateSubmission,
                    ClassId = request.ClassId,
                    SubjectId = request.SubjectId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Assignments.Add(assignment);
                await _context.SaveChangesAsync();

                var assignmentDto = new AssignmentDto
                {
                    Id = assignment.Id,
                    Title = assignment.Title,
                    Description = assignment.Description,
                    Deadline = assignment.Deadline,
                    MaxMarks = assignment.MaxMarks,
                    Status = assignment.Status,
                    AllowLateSubmission = assignment.AllowLateSubmission,
                    CreatedAt = assignment.CreatedAt,
                    ClassId = assignment.ClassId,
                    ClassName = classEntity.Name,
                    SubjectId = assignment.SubjectId,
                    SubjectName = subject.Name
                };

                return CreatedAtAction(nameof(GetAssignmentById), new { id = assignment.Id }, assignmentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating assignment");
                return StatusCode(500, new { message = "An error occurred while creating assignment" });
            }
        }

        [HttpPut("assignments/{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateAssignment(int id, [FromBody] UpdateAssignmentRequest request)
        {
            try
            {
                var teacherId = GetCurrentUserId();
                var assignment = await _context.Assignments
                    .Include(a => a.Subject)
                    .FirstOrDefaultAsync(a => a.Id == id && a.Subject.TeacherId == teacherId);

                if (assignment == null)
                {
                    return NotFound(new { message = "Assignment not found" });
                }

                if (!string.IsNullOrEmpty(request.Title))
                    assignment.Title = request.Title;
                if (!string.IsNullOrEmpty(request.Description))
                    assignment.Description = request.Description;
                if (request.Deadline.HasValue)
                    assignment.Deadline = request.Deadline.Value;
                if (request.MaxMarks.HasValue)
                    assignment.MaxMarks = request.MaxMarks.Value;
                if (request.Status.HasValue)
                    assignment.Status = request.Status.Value;
                if (request.AllowLateSubmission.HasValue)
                    assignment.AllowLateSubmission = request.AllowLateSubmission.Value;

                assignment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var classEntity = await _context.Classes.FindAsync(assignment.ClassId);
                var subject = await _context.Subjects.FindAsync(assignment.SubjectId);

                var assignmentDto = new AssignmentDto
                {
                    Id = assignment.Id,
                    Title = assignment.Title,
                    Description = assignment.Description,
                    Deadline = assignment.Deadline,
                    MaxMarks = assignment.MaxMarks,
                    Status = assignment.Status,
                    AllowLateSubmission = assignment.AllowLateSubmission,
                    CreatedAt = assignment.CreatedAt,
                    ClassId = assignment.ClassId,
                    ClassName = classEntity?.Name ?? "",
                    SubjectId = assignment.SubjectId,
                    SubjectName = subject?.Name ?? ""
                };

                return Ok(assignmentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating assignment");
                return StatusCode(500, new { message = "An error occurred while updating assignment" });
            }
        }

        [HttpDelete("assignments/{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            try
            {
                var teacherId = GetCurrentUserId();
                var assignment = await _context.Assignments
                    .Include(a => a.Subject)
                    .FirstOrDefaultAsync(a => a.Id == id && a.Subject.TeacherId == teacherId);

                if (assignment == null)
                {
                    return NotFound(new { message = "Assignment not found" });
                }

                _context.Assignments.Remove(assignment);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Assignment deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting assignment");
                return StatusCode(500, new { message = "An error occurred while deleting assignment" });
            }
        }

        // Submission Management
        [HttpGet("assignments/{assignmentId}/submissions")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetAssignmentSubmissions(int assignmentId)
        {
            try
            {
                var teacherId = GetCurrentUserId();
                var assignment = await _context.Assignments
                    .Include(a => a.Subject)
                    .FirstOrDefaultAsync(a => a.Id == assignmentId && a.Subject.TeacherId == teacherId);

                if (assignment == null)
                {
                    return NotFound(new { message = "Assignment not found" });
                }

                var submissions = await _context.Submissions
                    .Include(s => s.Student)
                    .Where(s => s.AssignmentId == assignmentId)
                    .Select(s => new SubmissionDto
                    {
                        Id = s.Id,
                        Answer = s.Answer,
                        Status = s.Status,
                        MarksObtained = s.MarksObtained,
                        Feedback = s.Feedback,
                        SubmittedAt = s.SubmittedAt,
                        ReviewedAt = s.ReviewedAt,
                        AssignmentId = s.AssignmentId,
                        AssignmentTitle = assignment.Title,
                        StudentId = s.StudentId,
                        StudentName = $"{s.Student.FirstName} {s.Student.LastName}",
                        StudentEmail = s.Student.Email
                    })
                    .ToListAsync();

                return Ok(submissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting submissions");
                return StatusCode(500, new { message = "An error occurred while retrieving submissions" });
            }
        }

        [HttpGet("submissions/{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetSubmissionById(int id)
        {
            try
            {
                var teacherId = GetCurrentUserId();
                var submission = await _context.Submissions
                    .Include(s => s.Assignment)
                    .ThenInclude(a => a.Subject)
                    .Include(s => s.Student)
                    .FirstOrDefaultAsync(s => s.Id == id && s.Assignment.Subject.TeacherId == teacherId);

                if (submission == null)
                {
                    return NotFound(new { message = "Submission not found" });
                }

                var submissionDto = new SubmissionDto
                {
                    Id = submission.Id,
                    Answer = submission.Answer,
                    Status = submission.Status,
                    MarksObtained = submission.MarksObtained,
                    Feedback = submission.Feedback,
                    SubmittedAt = submission.SubmittedAt,
                    ReviewedAt = submission.ReviewedAt,
                    AssignmentId = submission.AssignmentId,
                    AssignmentTitle = submission.Assignment.Title,
                    StudentId = submission.StudentId,
                    StudentName = $"{submission.Student.FirstName} {submission.Student.LastName}",
                    StudentEmail = submission.Student.Email
                };

                return Ok(submissionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting submission");
                return StatusCode(500, new { message = "An error occurred while retrieving submission" });
            }
        }

        [HttpPut("submissions/{id}/grade")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GradeSubmission(int id, [FromBody] GradeSubmissionRequest request)
        {
            try
            {
                var teacherId = GetCurrentUserId();
                var submission = await _context.Submissions
                    .Include(s => s.Assignment)
                    .ThenInclude(a => a.Subject)
                    .FirstOrDefaultAsync(s => s.Id == id && s.Assignment.Subject.TeacherId == teacherId);

                if (submission == null)
                {
                    return NotFound(new { message = "Submission not found" });
                }

                if (request.MarksObtained > submission.Assignment.MaxMarks)
                {
                    return BadRequest(new { message = "Marks obtained cannot exceed maximum marks" });
                }

                submission.MarksObtained = request.MarksObtained;
                submission.Feedback = request.Feedback;
                submission.Status = request.Status;
                submission.ReviewedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var submissionDto = new SubmissionDto
                {
                    Id = submission.Id,
                    Answer = submission.Answer,
                    Status = submission.Status,
                    MarksObtained = submission.MarksObtained,
                    Feedback = submission.Feedback,
                    SubmittedAt = submission.SubmittedAt,
                    ReviewedAt = submission.ReviewedAt,
                    AssignmentId = submission.AssignmentId,
                    AssignmentTitle = submission.Assignment.Title,
                    StudentId = submission.StudentId,
                    StudentName = $"{submission.Student.FirstName} {submission.Student.LastName}",
                    StudentEmail = submission.Student.Email
                };

                return Ok(submissionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error grading submission");
                return StatusCode(500, new { message = "An error occurred while grading submission" });
            }
        }

        [HttpPut("submissions/{id}/status")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateSubmissionStatus(int id, [FromBody] UpdateSubmissionStatusRequest request)
        {
            try
            {
                var teacherId = GetCurrentUserId();
                var submission = await _context.Submissions
                    .Include(s => s.Assignment)
                    .ThenInclude(a => a.Subject)
                    .FirstOrDefaultAsync(s => s.Id == id && s.Assignment.Subject.TeacherId == teacherId);

                if (submission == null)
                {
                    return NotFound(new { message = "Submission not found" });
                }

                submission.Status = request.Status;
                if (request.Status == SubmissionStatus.Reviewed && submission.ReviewedAt == null)
                {
                    submission.ReviewedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Submission status updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating submission status");
                return StatusCode(500, new { message = "An error occurred while updating submission status" });
            }
        }

        // Get teacher's classes and subjects
        [HttpGet("my-classes")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetMyClasses()
        {
            try
            {
                var teacherId = GetCurrentUserId();
                var classes = await _context.Classes
                    .Where(c => c.TeacherId == teacherId)
                    .Select(c => new ClassDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Code = c.Code,
                        Description = c.Description,
                        TeacherId = c.TeacherId,
                        TeacherName = $"{c.Teacher.FirstName} {c.Teacher.LastName}",
                        CreatedAt = c.CreatedAt
                    })
                    .ToListAsync();

                return Ok(classes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting teacher classes");
                return StatusCode(500, new { message = "An error occurred while retrieving classes" });
            }
        }

        [HttpGet("my-subjects")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetMySubjects()
        {
            try
            {
                var teacherId = GetCurrentUserId();
                var subjects = await _context.Subjects
                    .Where(s => s.TeacherId == teacherId)
                    .Select(s => new SubjectDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Code = s.Code,
                        Description = s.Description,
                        TeacherId = s.TeacherId,
                        TeacherName = $"{s.Teacher.FirstName} {s.Teacher.LastName}",
                        CreatedAt = s.CreatedAt
                    })
                    .ToListAsync();

                return Ok(subjects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting teacher subjects");
                return StatusCode(500, new { message = "An error occurred while retrieving subjects" });
            }
        }
    }
}
